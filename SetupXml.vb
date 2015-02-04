Imports System
Imports System.Xml
Imports Microsoft

Class SetupXml
    Public xml As XmlDocument
    Public nsman As XmlNamespaceManager

    Public Sub New()
        xml = New XmlDocument
        nsman = New XmlNamespaceManager(xml.NameTable)

        nsman.AddNamespace("atom", "http://www.w3.org/2005/Atom")
        nsman.AddNamespace("openSearch", "http://a9.com/-/spec/opensearchrss/1.0/")
        nsman.AddNamespace("apps", "http://schemas.google.com/apps/2006")
        nsman.AddNamespace("gd", "http://schemas.google.com/g/2005")
    End Sub
End Class
