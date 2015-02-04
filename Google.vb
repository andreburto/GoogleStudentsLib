Imports System
Imports System.IO
Imports System.Net
Imports System.Security.Cryptography.X509Certificates
Imports System.Text
Imports System.Text.Encoding
Imports System.Text.StringBuilder
Imports System.Web
Imports System.Xml
Imports Microsoft

Public Class Google

    Private googAuthkey As String
    Private googDomain As String
    Public urlA As String = "https://www.google.com/accounts/ClientLogin"
    Public urlB As String, urlC As String

    Public Sub New()
    End Sub

    Public Property authkey() As String
        Get
            Return Me.googAuthkey
        End Get
        Set(ByVal Value As String)
            Me.googAuthkey = Value
        End Set
    End Property

    Public Property domain() As String
        Get
            Return Me.googDomain
        End Get
        Set(ByVal Value As String)
            Me.googDomain = Value
            Me.urlB = "https://apps-apis.google.com/a/feeds/" + Value + "/user/2.0"
            Me.urlC = "https://apps-apis.google.com/a/feeds/group/2.0/" + Value
        End Set
    End Property

#Region " User functions "

    Public Function GetUserListLength(ByVal xml As String) As Integer
        Dim x As New SetupXml

        x.xml.LoadXml(xml)

        Return x.xml.SelectNodes("/atom:feed/atom:entry", x.nsman).Count
    End Function

    Public Function GetUserList(ByVal xml As String) As Array
        Dim tempArray As New ArrayList
        Dim x As New SetupXml

        x.xml.LoadXml(xml)

        For Each y As XmlNode In x.xml.SelectNodes("/atom:feed/atom:entry", x.nsman)
            If y.Item("title").InnerText.Length > 0 Then
                tempArray.Add(y.Item("title").InnerText)
            End If
        Next

        Return tempArray.ToArray
    End Function

    Public Function GetOneUser(ByVal url As String) As User
        Dim getit As HttpWebRequest = WebRequest.Create(url)
        Dim results As New User
        Dim temp As String

        getit.Method = "GET"
        getit.UserAgent = "WCCS Email"
        getit.ContentType = "application/atom+xml"
        getit.Headers.Add("Authorization", "GoogleLogin auth=" + googAuthkey)

        Try
            Dim resp As WebResponse = getit.GetResponse()
            Dim sr As StreamReader = New StreamReader(resp.GetResponseStream())
            temp = sr.ReadToEnd
            resp.Close()

            If (temp.IndexOf("AppsForYourDomainErrors") = -1) Then
                results = ParseUser(temp)
                results.exists = True
            Else
                results.id = temp
                results.exists = False
            End If
        Catch x As Exception
            results.id = x.Message
            results.exists = False
        End Try

        Return results
    End Function

    Public Function CheckUserExists(ByVal userid As String) As Boolean
        If userid.Length < 6 Then Return False
        Dim x As User = New User
        x = GetOneUser(Me.urlB + "/" + userid)
        Return x.exists
    End Function

    Public Function GetUsers(ByVal url As String) As String
        Dim getit As HttpWebRequest = WebRequest.Create(url)
        Dim results As String
        Dim data As New StringBuilder

        getit.Method = "GET"
        getit.UserAgent = "WCCS Email"
        getit.ContentType = "application/atom+xml"
        getit.Headers.Add("Authorization", "GoogleLogin auth=" + googAuthkey)

        Try
            Dim resp As WebResponse = getit.GetResponse()
            Dim sr As StreamReader = New StreamReader(resp.GetResponseStream())
            results = sr.ReadToEnd
            resp.Close()
        Catch x As Exception
            results = x.Message
        End Try

        Return results
    End Function

    Public Function GetAllUsers() As Students
        Dim userList As String = ""
        Dim studentList As Students = New Students

        userList = Me.GetUsers(Me.urlB)

        While Me.GetNextUrl(userList).Length > 0
            For Each student As String In Me.GetUserList(userList)
                If studentList.ContainsStudent(student) = False Then
                    studentList.AddStudent(student)
                End If
            Next
            Threading.Thread.Sleep(1000)
            Dim nextUrl As String = Me.GetNextUrl(userList)

            If nextUrl.Length > 0 Then
                userList = Me.GetUsers(nextUrl)
            End If
        End While

        Return studentList
    End Function

    Public Function CreateUser(ByVal id As String, ByVal fname As String, ByVal lname As String, _
                               ByVal password As String) As User
        Dim data As String = CreateUserXml(id, fname, lname, password)
        Dim temp As String = HandleUser(data, "POST", Me.urlB)
        Dim results As New User
        results.exists = False

        If temp.Length > 0 And temp.IndexOf("AppsForYourDomainErrors") = -1 Then
            results = ParseUser(temp)
            results.exists = True
        Else

        End If

        Return results
    End Function

    Public Function DeleteUser(ByVal userid As String) As Boolean
        If CheckUserExists(userid) = False Then Return True

        Dim url As String = Me.urlB + "/" + userid
        Dim getit As HttpWebRequest = WebRequest.Create(url)

        getit.Method = "DELETE"
        getit.UserAgent = "WCCS Email"
        getit.ContentType = "application/atom+xml"
        getit.Headers.Add("Authorization", "GoogleLogin auth=" + googAuthkey)

        Try
            Dim resp As HttpWebResponse = getit.GetResponse()
            If resp.StatusCode = HttpStatusCode.OK Then Return True
        Catch x As Exception
            Return False
        End Try

        Return False
    End Function

    Public Function UpdateUser(ByVal id As String, ByVal fname As String, ByVal lname As String, _
                               ByVal password As String) As User
        Dim url As String = Me.urlB + "/" + id
        Dim data As String = CreateUserXml(id, fname, lname, password)
        Dim temp As String = HandleUser(data, "PUT", url)
        Dim results As New User
        results.exists = False

        If temp.Length > 0 And temp.IndexOf("AppsForYourDomainErrors") = -1 Then
            results = ParseUser(temp)
            results.exists = True
        End If

        Return results
    End Function

    Private Function ParseUser(ByVal xml As String) As User
        Dim x As New SetupXml
        Dim results As New User

        Try
            x.xml.LoadXml(xml)
            results.id = x.xml.SelectSingleNode("/atom:entry/atom:title", x.nsman).InnerText
            results.fname = x.xml.SelectSingleNode("/atom:entry/apps:name", x.nsman).Attributes("familyName").Value
            results.lname = x.xml.SelectSingleNode("/atom:entry/apps:name", x.nsman).Attributes("givenName").Value
            results.exists = True
        Catch ex As Exception
            results.exists = False
        End Try

        Return results
    End Function

    Private Function HandleUser(ByVal xml As String, ByVal method As String, ByVal url As String) As String
        Dim getit As HttpWebRequest = WebRequest.Create(url)
        Dim temp As String

        getit.ContentLength = xml.Length
        getit.Method = method
        getit.UserAgent = "WCCS Email"
        getit.ContentType = "application/atom+xml"
        getit.Headers.Add("Authorization", "GoogleLogin auth=" + googAuthkey)

        Dim DataStream As New StreamWriter(getit.GetRequestStream())
        DataStream.Write(xml, 0, xml.Length)
        DataStream.Close()

        Try
            Dim resp As WebResponse = getit.GetResponse()
            Dim sr As StreamReader = New StreamReader(resp.GetResponseStream())
            temp = sr.ReadToEnd
            resp.Close()
        Catch x As Exception
            temp = ""
        End Try

        Return temp
    End Function

    Private Function CreateUserXml(ByVal id As String, ByVal fname As String, ByVal lname As String, _
                                   ByVal password As String) As String
        Dim data As New StringBuilder

        data.Append("<?xml version=""1.0"" encoding=""UTF-8""?>")
        data.Append("<atom:entry xmlns:atom=""http://www.w3.org/2005/Atom"" xmlns:apps=""http://schemas.google.com/apps/2006"">")
        data.Append("<atom:category scheme=""http://schemas.google.com/g/2005#kind"" term=""http://schemas.google.com/apps/2006#user""/>")
        data.Append("<apps:login userName=""" + id + """ password=""" + password + """ suspended=""false""/>")
        data.Append("<apps:name familyName=""" + lname + """ givenName=""" + fname + """/>")
        data.Append("</atom:entry>")

        Return data.ToString
    End Function

#End Region

#Region " Group functions "

    Public Function CreateGroup(ByVal id As String, ByVal name As String, ByVal description As String, ByVal perm As String) As Group
        Dim data As String = CreateGroupXml(id, name, description, perm)
        Dim temp As String = HandleUser(data, "POST", Me.urlB)
        Dim x As New SetupXml
        Dim results As New Group

        If temp.IndexOf("AppsForYourDomainErrors") > -1 Then
            ErrorMsg.Show("Could not create group.")
            Return results
        End If

        x.xml.LoadXml(temp)
        Dim xList As XmlNodeList = x.xml.SelectNodes("/atom:entry/atom:property", x.nsman)
        results.id = FindGroupAttribute("groupId", xList)
        results.name = FindGroupAttribute("groupName", xList)
        results.description = FindGroupAttribute("description", xList)
        results.emailPermission = FindGroupAttribute("emailPermission", xList)
        Return results
    End Function

    Public Function AddMemberToGroup(ByVal groupId As String, ByVal userId As String) As Boolean
        Dim xml As String = AddMemberToGroupXml(userId)
        Dim tempUrl As String = Me.urlC + "/" + groupId + "/member"
        Dim temp As String = HandleUser(xml, "POST", tempUrl)
        Dim x As New SetupXml
        Dim retval As Boolean = False

        If temp.IndexOf("AppsForYourDomainErrors") > -1 Then
            ErrorMsg.Show("Could not create group.")
            Return retval
        End If

        x.xml.LoadXml(temp)
        Dim xList As XmlNodeList = x.xml.SelectNodes("/atom:entry/atom:property", x.nsman)
        If FindGroupAttribute("memberId", xList) Is "" Then retval = False Else retval = True
        Return retval
    End Function

    Public Function RemoveMemberFromGroup(ByVal groupId As String, ByVal userId As String) As Boolean
        Dim tempUrl As String = Me.urlC + "/" + groupId + "/member/" + userId
        Dim temp As String = HandleUser("", "DELETE", tempUrl)
        Dim x As New SetupXml
        Dim retval As Boolean = False

        If temp.IndexOf("AppsForYourDomainErrors") > -1 Then
            ErrorMsg.Show("Could not create group.")
            Return retval
        End If

        x.xml.LoadXml(temp)
        Dim xList As XmlNodeList = x.xml.SelectNodes("/atom:entry/atom:property", x.nsman)
        If FindGroupAttribute("memberId", xList) Is "" Then retval = False Else retval = True
        Return retval
    End Function

    Private Function FindGroupAttribute(ByVal key As String, ByVal x As XmlNodeList) As String
        Dim temp As String = ""
        For Each xnode As XmlNode In x
            Dim name As String = xnode.Attributes("name").Value
            If key = name Then
                temp = xnode.Attributes("value").Value
                Exit For
            End If
        Next
        Return temp
    End Function

    Private Function CreateGroupXml(ByVal id As String, ByVal name As String, ByVal description As String, ByVal perm As String) As String
        Dim data As New StringBuilder
        data.Append("<?xml version=""1.0"" encoding=""UTF-8""?>")
        data.Append("<atom:entry xmlns:atom=""http://www.w3.org/2005/Atom"" xmlns:apps=""http://schemas.google.com/apps/2006"" xmlns:gd=""http://schemas.google.com/g/2005"">")
        data.Append("<apps:property name=""groupId"" value=""" + id + """></apps:property>")
        data.Append("<apps:property name=""groupName"" value=""" + name + """></apps:property>")
        data.Append("<apps:property name=""description"" value=""" + description + """></apps:property>")
        data.Append("<apps:property name=""emailPermission"" value=""" + perm + """></apps:property>")
        data.Append("</atom:entry>")
        Return data.ToString
    End Function

    Private Function AddMemberToGroupXml(ByVal email As String) As String
        Dim data As New StringBuilder
        data.Append("<?xml version=""1.0"" encoding=""UTF-8""?>")
        data.Append("<atom:entry xmlns:atom=""http://www.w3.org/2005/Atom"" xmlns:apps=""http://schemas.google.com/apps/2006"" xmlns:gd=""http://schemas.google.com/g/2005"">")
        data.Append("<apps:property name=""memberId"" value=""" + email + """></apps:property>")
        data.Append("</atom:entry>")
        Return data.ToString
    End Function

#End Region

#Region " General Google Functions "

    Public Function GetNextUrl(ByVal xml As String) As String
        Dim url As String = ""
        Dim x As New SetupXml

        Try
            x.xml.LoadXml(xml)

            For Each y As XmlNode In x.xml.SelectNodes("/atom:feed/atom:link", x.nsman)
                If y.Attributes("rel").Value.ToLower = "next" Then
                    url = y.Attributes("href").Value
                    Exit For
                End If
            Next
        Catch ex As Exception
            ErrorMsg.Show(xml)
        End Try

        Return url
    End Function

    Public Function GetAuth(ByVal email As String, ByVal pword As String) As String

        Dim getit As HttpWebRequest = WebRequest.Create(urlA)
        Dim results As String
        Dim data As New StringBuilder

        data.Append("Email=" + System.Web.HttpUtility.UrlEncode(email))
        data.Append("&Passwd=" + System.Web.HttpUtility.UrlEncode(pword))
        data.Append("&accountType=HOSTED&service=apps")

        getit.ContentLength = data.Length()
        getit.Method = "POST"
        getit.UserAgent = "WCCS Email"
        getit.ContentType = "application/x-www-form-urlencoded"

        Dim DataStream As New StreamWriter(getit.GetRequestStream())
        DataStream.Write(data.ToString, 0, data.Length)
        DataStream.Close()

        Try
            Dim resp As WebResponse = getit.GetResponse()
            Dim sr As StreamReader = New StreamReader(resp.GetResponseStream())
            results = sr.ReadToEnd
            resp.Close()

            For Each line As String In results.Split(Microsoft.VisualBasic.vbLf)
                Dim keyval() As String = line.Split(New Char() {"="}, 2)

                If keyval(0).ToLower = "auth" Then
                    results = keyval(1)
                    Exit For
                End If
            Next
        Catch x As Exception
            results = "ERROR: " + x.Message
        End Try

        Return results
    End Function

#End Region

End Class