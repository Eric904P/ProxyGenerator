Imports System.IO
Imports System.Net
Imports System.Threading
Imports ProxyGenerator.My.Resources

Public Class Checker
    '///////////////START OF CHECKER BLOCK/////////////////
    Dim _thrdCntC As Integer = 0
    ReadOnly _thrdMaxC As Integer = 50
    ReadOnly _listLockC As Object = New Object
    ReadOnly _dC As New Dictionary(Of String, Thread)()
    'ReadOnly _working As List(Of String) = New List(Of String)
    Private _scrapedTotal As Integer = 0
    Private _fileIndex As Integer = 0
    Private _scraped = Scraper.LoadScraped(true)
    Private _fileLock As Object = New Object()

    Public Sub New()

    End Sub

    Function CheckHerder() As Boolean
        _scrapedTotal = _scraped.Count
        Dim thrdIndexC = 1
        While _scraped.Count > 0
            If _thrdCntC <= _thrdMaxC Then
                _dC(thrdIndexC.ToString) = New Thread(AddressOf CheckTask)
                _dC(thrdIndexC.ToString).IsBackground = True
                _dC(thrdIndexC.ToString).Start()
                _thrdCntC = _thrdCntC + 1
                thrdIndexC = thrdIndexC + 1
            End If
        End While
        Thread.Sleep(20000)
        Return True
    End Function

    Private Sub CheckTask()
        If _scraped.Count > 0 Then
            Dim toCheck As String
            SyncLock _listLockC
                toCheck = _scraped.Item(0)
                _scraped.RemoveAt(0)
            End SyncLock
            If CheckProxy(toCheck) Then
                '_working.Add(toCheck)
                SaveChecked(toCheck)
                Console.WriteLine(toCheck)
            End If
            _thrdCntC = _thrdCntC - 1
        End If
    End Sub

    'test single proxy
    Function CheckProxy(proxy As String) As Boolean
        Try 'uses azenv.net proxy judge
            Dim r As HttpWebRequest = HttpWebRequest.Create(Judge)
            r.UserAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.2 Safari/537.36"
            r.Timeout = 3000
            r.ReadWriteTimeout = 3000
            r.Proxy = New WebProxy(proxy)
            Using sr As New StreamReader(r.GetResponse().GetResponseStream())
                If sr.ReadToEnd().Contains("HTTP_HOST = azenv.net") Then
                    r.Abort()
                    Return True
                End If
            End Using
            r.Abort()
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function

    Private Sub SaveChecked(chk As String)
        Dim TmpPath As String = Path.GetTempPath() + "ProxyGenCheck_" + _fileIndex.toString()
        Dim maxSize As Long = 100000
        SyncLock _fileLock
            Try
                If (FileLen(TmpPath) > maxSize) Then
                'If (File.ReadAllLines(TmpPath).Length > 2500) Then
                _fileIndex = _fileIndex + 1
                TmpPath = Path.GetTempPath() + "ProxyGenCheck_" + _fileIndex.toString()
            End If
            Catch Ex as Exception
                'File not created yet
            End Try
            Using SW = New StreamWriter(TmpPath, true)
                    SW.WriteLine(chk)
            End Using
        End SyncLock
    End Sub

    Public Function LoadChecked(bool As Boolean) As List(Of String)
        Dim checked As List(Of String) = New List(Of String)
        For Each f As String In Directory.GetFiles(Path.GetTempPath())
            If f.Contains("ProxyGenCheck_") Then
                Using SR = New StreamReader(f)
                    checked.AddRange(SR.ReadToEnd().Split(vbNewLine).ToList())
                End Using
                If bool Then
                    File.Delete(f)
                End If
            End If 
        Next
        Return checked
    End Function

    'Public Function ReturnWorking() As List(Of String)
    '    Return _working
    'End Function

    Public Function ReturnThreadCount() As Integer
        Return _thrdCntC
    End Function

    Public Function ReturnPercent() As Double
        Return ((_scrapedTotal - _scraped.Count) / _scrapedTotal)
    End Function

    Public Function ReturnCheckedCount() As Integer
        Return (_scrapedTotal - _scraped.Count)
    End Function

    Public Function ReturnScrapedTotal() As Integer
        Return _scrapedTotal
    End Function
End Class