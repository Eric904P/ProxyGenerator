Imports System.IO
Imports System.Net
Imports System.Threading
Imports ProxyGenerator.My.Resources

Public Class Checker
    Dim _thrdCntC As Integer = 0
    ReadOnly _thrdMaxC As Integer = 50
    ReadOnly _listLockC As Object = New Object
    ReadOnly _dC As New Dictionary(Of String, Thread)()
    Private _scrapedTotal As Integer = 0
    Private _fileIndex As Integer = 0
    Private _workingCount As Integer = 0
    Private _scraped As List(Of String) = New List(Of String) 'Scraper.LoadScraped(true)
    Private _fileLock As Object = New Object()
    Private _screenLock As Object = New Object()
    Dim _lineIndex As Integer = 0
    Private _working As List(Of String) = New List(Of String)
    Dim _fileManager, _loadBuffer As Thread 

    'constructor
    Public Sub New()

    End Sub

    'thread manager
    Function CheckHerder() As Boolean
        Console.SetCursorPosition(left:=0, top:=0)
        Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
        Console.WriteLine("/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\ PROXY GENERATOR v2.5 /\/\/\/\/\/\/\/\/\/\/\/\/\/\")
        Console.WriteLine(" /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\ BY ERIC904P /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\")
        Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
        Console.WriteLine("- - - - - - - - - - - - - - - -CHECKING PLEASE WAIT- - - - - - - - - - - - - - -")
        Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
        Console.SetCursorPosition(0, 8)
        Console.CursorVisible = False
        _scrapedTotal = _scraped.Count
        Dim thrdIndexC = 1
        _fileManager = New Thread(AddressOf CheckedBuffer)
        _fileManager.IsBackground = True
        _fileManager.Start()
        _loadBuffer = New Thread(AddressOf ScrapedBufferLoader)
        _loadBuffer.IsBackground = True
        _loadBuffer.Start()
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

    'each thread does this action
    Private Sub CheckTask()
        If _scraped.Count > 0 Then
            Dim toCheck As String
            SyncLock _listLockC
                toCheck = _scraped.Item(0)
                _scraped.RemoveAt(0)
            End SyncLock
            If CheckProxy(toCheck) Then
                _working.Add(toCheck)
                _workingCount = _workingCount + 1
                SyncLock _screenLock
                    Console.SetCursorPosition(0,8)
                    Console.WriteLine(Math.Abs(Math.Round((1-(_scraped.Count/_scrapedTotal))*100, 2)) & "%                                   ")
                End SyncLock
                'Console.WriteLine(toCheck)
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

    'splits working proxy output into 2500 line files
    Private Sub CheckedBuffer()
        Dim tmpPath As String = Path.GetTempPath() + "ProxyGenCheck_" + _fileIndex.toString()
        While _thrdCntC > 0
            If _working.Count > 2500 Then
                File.AppendAllLines(tmpPath, _working.GetRange(0, 2500))
                _fileIndex = _fileIndex + 1
                tmpPath = Path.GetTempPath() + "ProxyGenCheck_" + _fileIndex.ToString()
                _working.RemoveRange(0, 2500)
            End If
        End While
        If _working.Count > 0 And _working.Count < 2500 Then
            File.AppendAllLines(tmpPath, _working)
            _fileIndex = _fileIndex + 1
            tmpPath = Path.GetTempPath() + "ProxyGenCheck_" + _fileIndex.ToString()
            _working.Clear()
        End If
    End Sub

    'rejoins split files for saving
    Public Function LoadChecked(bool As Boolean) As List(Of String)
        Dim checked As List(Of String) = New List(Of String)
        For Each f As String In Directory.GetFiles(Path.GetTempPath())
            If f.Contains("ProxyGenCheck_") Then
                Using sr = New StreamReader(f)
                    checked.AddRange(sr.ReadToEnd().Split(vbNewLine).ToList())
                End Using
                If bool Then
                    File.Delete(f)
                End If
            End If 
        Next
        Return checked
    End Function

    Private Sub ScrapedBufferLoader()
        Dim buffIndex As Integer = 0
        Dim tmpPath As String = Path.GetTempPath() + "ProxyGenScrape_" + buffIndex.toString()
        Dim fileEnd As Boolean = False
        While _thrdCntC > 0 and Not fileEnd
            If Not File.Exists(tmpPath) Then 
                fileEnd = True
                End
            End If
            If _scraped.Count < 1000 Then
                _scraped.AddRange(File.ReadAllLines(tmpPath))
                File.Delete(tmpPath)
                buffIndex = buffIndex + 1
                tmpPath = Path.GetTempPath() + "ProxyGenScrape_" + buffIndex.ToString()
            End If
        End While        
    End Sub
End Class