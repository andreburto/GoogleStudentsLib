Imports System
Imports System.Net
Imports System.Security.Cryptography.X509Certificates

'This class handles problems with certificates if ssl (https) is used
' http://www.xtremedotnettalk.com/showthread.php?threadid=80674
Class MyCertificateValidation
    Implements System.Net.ICertificatePolicy

    Public Function CheckValidationResult(ByVal srvPoint As ServicePoint, _
    ByVal cert As X509Certificate, ByVal request As WebRequest, ByVal problem As Integer) _
       As Boolean Implements ICertificatePolicy.CheckValidationResult
        Return True
    End Function
End Class
