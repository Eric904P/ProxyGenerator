Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows.Forms
Imports ProxyGen.My.Resources

Module Module1
    Dim _sources As List(Of String) = New List(Of String)
    Dim _working As List(Of String) = New List(Of String)
    Private ReadOnly _screenLock As Object = New Object()
    Private ReadOnly _listLock As Object = New Object()
    Private ReadOnly _fileLock As Object = New Object()
    Dim _thrdCntS As Integer = 0
    ReadOnly _thrdMaxS As Integer = 50
    ReadOnly _dS As New Dictionary(Of String, Thread)()
    Private _scrapedTotal As Integer = 0
    Private _sourceTotal As Integer = 0

    Function ScrapeHerder() As Boolean
        LoadSrc()
        Console.SetCursorPosition(left:=0, top:=0)
        Console.WriteLine(ScrapeHeader)
        'Console.WriteLine("/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\ PROXY GENERATOR v2.5 /\/\/\/\/\/\/\/\/\/\/\/\/\/\")
        'Console.WriteLine(" /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\ BY ERIC904P /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\")
        'Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
        'Console.WriteLine("- - - - - - - - - - - - - - - -SCRAPING PLEASE WAIT- - - - - - - - - - - - - - -")
        'Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
        Console.WriteLine(_sources.Count.ToString() + " sources loaded")
        'Console.SetCursorPosition(0, 7)
        Console.CursorVisible = False
        _sourceTotal = _sources.Count
        Dim thrdIndex = 1
        While _sources.Count > 0
            If _thrdCntS <= _thrdMaxS Then
                _dS(thrdIndex.ToString) = New Thread(AddressOf ScrapeTask)
                _dS(thrdIndex.ToString).IsBackground = True
                _dS(thrdIndex.ToString).Start()
                _thrdCntS = _thrdCntS + 1
                thrdIndex = thrdIndex + 1
            End If
        End While
        Thread.Sleep(20000)
        Return True
    End Function

    ''' <summary>
    '''     Task for scraper thread
    ''' </summary>
    Private Sub ScrapeTask()
        If _sources.Count > 0 Then
            Dim toScrape As String
            SyncLock _listLock
                toScrape = _sources.Item(0)
                _sources.RemoveAt(0)
            End SyncLock
            Dim thrdIndex = 1
            For Each p As String in ScrapeLink(toScrape).Distinct().ToList()
                If CheckProxy(p) Then
                    _working.Add(p)
                    SyncLock _screenLock
                        Console.WriteLine(p.ToString())
                    End SyncLock
                End If
            Next
            '_scraped.AddRange(ScrapeLink(toScrape).Distinct().ToList())
            _thrdCntS = _thrdCntS - 1
        End If
    End Sub

    ''' <summary>
    '''     Scrapes URL 'link' for proxies and returns found proxies as a List(Of String)
    ''' </summary>
    ''' <param name="link"></param>
    ''' <returns></returns>
    Private Function ScrapeLink(link As String) As List(Of String)
        Dim proxies = New List(Of String)()
        Try 
            proxies = ExtractProx(New StreamReader(HttpWebRequestWrapper.CreateNew(link, 15000, UserAgent).GetResponse().GetResponseStream()).ReadToEnd())
        Catch E As Exception
            writeLog(E.ToString())
        End Try
        Return proxies
        'If proxies.Count > 0 Then
        '    SyncLock _screenLock
        '        Console.SetCursorPosition(0, 7)
        '        Console.WriteLine(
        '            Math.Abs(Math.Round((1 - (_sources.Count / _sourceTotal)) * 100, 2)) &
        '            "%                                   ")
        '    End SyncLock
        'End If
    End Function

    ''' <summary>
    '''     Finds all proxies in a given string, returns them as a List(Of String)
    ''' </summary>
    ''' <param name="http"></param>
    ''' <returns></returns>
    Private Function ExtractProx(http As String) As List(Of String)
        Dim output = New List(Of String)

        For Each proxy As Match In Regex.Matches(http, "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+:[0-9]+")
            output.Add(proxy.ToString())
        Next
        Return output
    End Function

    ''' <summary>
    ''' Tests proxy connection using a judge
    ''' </summary>
    ''' <param name="proxy"></param>
    ''' <returns></returns>
    Function CheckProxy(proxy As String) As Boolean
        Try
            If New StreamReader(HttpWebRequestWrapper.CreateNew(Judge, time := 3000, ua := UserAgent, ioTime := 3000, yxorp := New WebProxy(Proxy)).GetResponse().GetResponseStream()).ReadToEnd().Contains(TestString) Then
                Return True
            End If
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function

    ''' <summary>
    '''     loads all source links from local resources
    ''' </summary>
    Private Sub LoadSrc()
        Dim psrc As String = My.Resources.psrc
        _sources = psrc.Split("$").ToList()
        If Not _sources.Count > 0 Then
            LoadSrcWeb()
        End If
        _sources.RemoveAt(0)
    End Sub

    ''' <summary>
    '''     fallback source method
    ''' </summary>
    Private Sub LoadSrcWeb()
        Dim client = New WebClient()
        Dim reader = New StreamReader(client.OpenRead(WebSrc))
        Dim temp As String = reader.ReadToEnd
        Dim tmpSrc As String() = temp.Split("$")
        _sources = tmpSrc.ToList()
        _sources.RemoveAt(0)
    End Sub

    Private Sub writeLog(log As string)
        SyncLock _fileLock
            File.AppendAllText(Path.GetTempPath() + "ProxyGen.LOGFILE", log + vbNewLine) 
        End SyncLock      
    End Sub

    Sub SaveFile(tempL As List(Of String))
        If (tempL.Any()) Then
            Dim fs As New SaveFileDialog
            fs.RestoreDirectory = True
            fs.Filter = v2_Threading_SaveFile_txt_files____txt____txt
            fs.FilterIndex = 1
            fs.ShowDialog()
            If Not (fs.FileName = Nothing) Then
                Using sw As New StreamWriter(fs.FileName)
                    For Each line As String In tempL
                        sw.WriteLine(line)
                    Next
                End Using
            End If
        Else
            Console.WriteLine(v2_Threading_SaveFile_Please_wait_until_program_is_finished_)
        End If
    End Sub

    Sub Main()
        If ScrapeHerder() Then
            SaveFile(_working)
        End If
    End Sub

End Module
