Public Class Member
    Public groupId As String = ""
    Public memberId As String = ""
    Public memberType As String = ""
    Public directMember As Boolean = False

    Public Sub New()
    End Sub

    Public Sub New(ByVal gid As String, ByVal mid As String, ByVal mtype As String, ByVal dir As String)
        Me.groupId = gid
        Me.memberId = mid
        Me.memberType = mtype
        If dir.ToLower Is "true" Then Me.directMember = True Else Me.directMember = False
    End Sub

    Public Sub New(ByVal gid As String, ByVal mid As String, ByVal mtype As String, ByVal dir As Boolean)
        Me.groupId = gid
        Me.memberId = mid
        Me.memberType = mtype
        Me.directMember = dir
    End Sub
End Class
