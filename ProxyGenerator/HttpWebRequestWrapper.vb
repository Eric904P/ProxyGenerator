Imports System.Net
Imports System.Net.Cache
Imports System.Net.Security
Imports System.Runtime.Serialization
Imports System.Security.Cryptography.X509Certificates
Imports System.Security.Principal

Class HttpWebRequestWrapper
    Inherits WebRequest
    Implements ISerializable

    ''' <summary>
    ''' Workaround for obsoltete Sub New()
    ''' </summary>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Allows for all options to be included in the inital constructor
    ''' </summary>
    ''' <param name="uri"></param>
    ''' <param name="time"></param>
    ''' <param name="ua"></param>
    ''' <param name="ioTime"></param>
    ''' <param name="yxorp"></param>
    ''' <returns></returns>
    Public Shared Function CreateNew(uri As String, Optional time As Integer = 5000, Optional ua As String = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.2 Safari/537.36", Optional ioTime As Integer = 5000, Optional yxorp As IWebProxy = Nothing) As HttpWebRequest
        Dim tempRequest As HttpWebRequest = HttpWebRequest.Create(uri)
        TempRequest.Timeout = time
        TempRequest.UserAgent = ua
        TempRequest.ReadWriteTimeout = IOTime
        TempRequest.Proxy = yxorp

        Return TempRequest
    End Function
End Class
