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
    ''' <param name="IOTime"></param>
    ''' <param name="yxorp"></param>
    ''' <returns></returns>
    Public Shared Function CreateNew(uri As String, Optional time As Integer, Optional ua As String, Optional IOTime As Integer, Optional yxorp As IWebProxy) As HttpWebRequest
        Dim tempRequest As HttpWebRequest = HttpWebRequest.Create(uri)
        TempRequest.Timeout = time
        TempRequest.UserAgent = ua
        TempRequest.ReadWriteTimeout = IOTime
        TempRequest.Proxy = yxorp

        Return TempRequest
    End Function
End Class
