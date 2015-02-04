Public Class User
    Public id As String = ""
    Public fname As String = ""
    Public lname As String = ""
    Public exists As Boolean = False

    Public Sub New()
    End Sub

    Public Sub New(ByVal i As String, ByVal f As String, ByVal l As String, ByVal e As Boolean)
        Me.id = i
        Me.fname = f
        Me.lname = l
        Me.exists = e
    End Sub
End Class
