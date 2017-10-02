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
    Private ReadOnly _scraped As List(Of String) = New List(Of String) 'Scraper.LoadScraped(true)
    Private _fileLock As Object = New Object()
    Private ReadOnly _screenLock As Object = New Object()
    Dim _lineIndex As Integer = 0
    Private ReadOnly _working As List(Of String) = New List(Of String)
    Dim _fileManager, _loadBuffer As Thread

    ''' <summary>
    ''' Default constructor
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' Main function called to check proxies
    ''' </summary>
    ''' <returns></returns>
    Function CheckHerder() As Boolean
        Console.SetCursorPosition(left := 0, top := 0)
        Console.WriteLine(CheckHeader)
        'Console.WriteLine("/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\ PROXY GENERATOR v2.5 /\/\/\/\/\/\/\/\/\/\/\/\/\/\")
        'Console.WriteLine(" /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\ BY ERIC904P /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\")
        'Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
        'Console.WriteLine("- - - - - - - - - - - - - - - -CHECKING PLEASE WAIT- - - - - - - - - - - - - - -")
        'Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
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

    ''' <summary>
    ''' Task for each checker thread
    ''' </summary>
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
                    Console.SetCursorPosition(0, 8)
                    Console.WriteLine(
                        Math.Abs(Math.Round((1 - (_scraped.Count/_scrapedTotal))*100, 2)) &
                        "%                                   ")
                End SyncLock
            End If
            _thrdCntC = _thrdCntC - 1
        End If
    End Sub

    ''' <summary>
    ''' Tests proxy connection using a judge
    ''' </summary>
    ''' <param name="proxy"></param>
    ''' <returns></returns>
    Function CheckProxy(proxy As String) As Boolean
        Try
            If New StreamReader(HttpWebRequestWrapper.CreateNew(Judge, time := 3000, ua := UserAgent, IOTime := 3000, yxorp := New WebProxy(Proxy)).GetResponse().GetResponseStream()).ReadToEnd().Contains(TestString) Then
                Return True
            End If
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Stores working proxies locally to save on memory
    ''' </summary>
    Private Sub CheckedBuffer()
        Dim tmpPath As String = Path.GetTempPath() + PGC_ + _fileIndex.toString()
        While _thrdCntC > 0
            If _working.Count > 2500 Then
                File.AppendAllLines(tmpPath, _working.GetRange(0, 2500))
                _fileIndex = _fileIndex + 1
                tmpPath = Path.GetTempPath() + PGC_ + _fileIndex.ToString()
                _working.RemoveRange(0, 2500)
            End If
        End While
        If _working.Count > 0 And _working.Count < 2500 Then
            File.AppendAllLines(tmpPath, _working)
            _fileIndex = _fileIndex + 1
            tmpPath = Path.GetTempPath() + PGC_ + _fileIndex.ToString()
            _working.Clear()
        End If
    End Sub

    ''' <summary>
    ''' Rejoin the local files, if "bool" then delete the files
    ''' </summary>
    ''' <param name="bool"></param>
    ''' <returns></returns>
    Public Function LoadChecked(bool As Boolean) As List(Of String)
        Dim checked = New List(Of String)
        For Each f As String In Directory.GetFiles(Path.GetTempPath())
            If f.Contains(PGC_) Then
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

    ''' <summary>
    ''' Read directly from local storage created by scraper
    ''' </summary>
    Private Sub ScrapedBufferLoader()
        Dim buffIndex = 0
        Dim tmpPath As String = Path.GetTempPath() + PGS_ + buffIndex.toString()
        Dim fileEnd = False
        While _thrdCntC > 0 and Not fileEnd
            If Not File.Exists(tmpPath) Then
                fileEnd = True
                End
            End If
            If _scraped.Count < 1000 Then
                _scraped.AddRange(File.ReadAllLines(tmpPath))
                File.Delete(tmpPath)
                buffIndex = buffIndex + 1
                tmpPath = Path.GetTempPath() + PGS_ + buffIndex.ToString()
            End If
        End While
    End Sub
End Class