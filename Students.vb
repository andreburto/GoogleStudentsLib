Imports System
Imports System.Collections
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class Students

    Private students As ArrayList

    Public Sub New()
        students = New ArrayList
    End Sub

    Public Function AddStudent(ByVal id As String) As Boolean
        If ContainsStudent(id) = False Then
            students.Add(id)
            Return True
        Else
            Return False
        End If
    End Function

    Public Function ContainsStudent(ByVal id As String) As Boolean
        Dim yesno As Boolean = False

        For Each student As String In students.ToArray
            If student = id Then
                yesno = True
                Exit For
            End If
        Next

        Return yesno
    End Function

    Public Function ListStudents() As Array
        Return students.ToArray
    End Function

End Class
