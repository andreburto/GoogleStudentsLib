Public Class Group

    Public id As String = ""
    Public name As String = ""
    Public description As String = ""
    Public emailPermission As String = ""

    Public Sub New()
    End Sub

    Public Sub New(ByVal i As String, ByVal n As String, ByVal d As String, ByVal e As String)
        Me.id = i
        Me.name = n
        Me.description = d
        Me.emailPermission = e
    End Sub
End Class
