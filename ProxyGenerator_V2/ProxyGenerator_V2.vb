Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Threading
Imports ProxyGenerator_V2.My.Resources

Public Class ProxyGenerator_V2
    Private _thrdCntScrape = 0
    Private _maxThrdScrape As Integer
    Private _thrdCntCheck = 0
    Private _maxThrdCheck As Integer
    Private _scrapedTotal = 0
    Private _sourceTotal = 0
    Private _sources As List(Of String) = New List(Of String)
    Private _scraped As List(Of String) = New List(Of String)
    Private _working As List(Of String) = New List(Of String)
    Private _hasResult As List(Of String) = New List(Of String)
    Private _noResult As List(Of String) = New List(Of String)
    Private _listLock = New Object
    Private _dictScrape = New Dictionary(Of String, Thread)()
    Private _dictCheck = New Dictionary(Of String, Thread)()
    Private _running As Boolean = False
    Private _paused As Boolean = False

    'delegate sub EventHandler()
    Event ProxyChecked() 'As EventHandler 'ByVal sender As System.Object
    Event SourceScraped() 'As EventHandler 'ByVal sender As System.Object

    Public Sub New()
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
    End Sub

    'SCRAPER
    Function ScrapeHerder() As Boolean
        'AddHandler SourceScraped, New SourceScrapedEventHandler(AddressOf StepPB1)
        _sourceTotal = _sources.Count
        Dim thrdIndex = 1
        While _sources.Any() And _running
            While _paused

            End While
            If _thrdCntScrape <= _maxThrdScrape Then
                _dictScrape(thrdIndex.ToString) = New Thread(AddressOf ScrapeTask)
                _dictScrape(thrdIndex.ToString).IsBackground = True
                _dictScrape(thrdIndex.ToString).Start()
                _thrdCntScrape = _thrdCntScrape + 1
                thrdIndex = thrdIndex + 1
                RaiseEvent SourceScraped()
            End If
        End While
        Return True
    End Function

    Private Sub ScrapeTask()
        If _sources.Any() Then
            Dim toScrape As String
            SyncLock _listLock
                toScrape = _sources.Item(0)
                _sources.RemoveAt(0)
            End SyncLock
            _scraped.AddRange(ScrapeLink(toScrape).Distinct().ToList())
            _thrdCntScrape = _thrdCntScrape - 1
        End If
    End Sub

    'scrapes a given link for proxies
    Private Function ScrapeLink(link As String) As List(Of String)
        Dim proxies = New List(Of String)
        Try 'gets the entire web page as a string
            Dim r As HttpWebRequest = HttpWebRequest.Create(link)
            r.UserAgent =
                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
            r.Timeout = 15000
            Using sr As New StreamReader(r.GetResponse().GetResponseStream())
                proxies = ExtractProx(sr.ReadToEnd())
            End Using
            r.Abort()
            If proxies.Any() Then
                _hasResult.Add(link)
            Else
                _noResult.Add(link)
            End If
        Catch ex As Exception

        End Try
        'returns scraped result
        Return proxies
    End Function

    'finds all proxies in a given string, returns them as a List(Of String)
    Private Function ExtractProx(http As String) As List(Of String)
        Dim output = New List(Of String)

        For Each proxy As Match In Regex.Matches(http, "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+:[0-9]+")
            output.Add(proxy.ToString())
        Next
        Return output
    End Function

    'loads all source links from internal resources
    Private Sub LoadSrc()
        Dim psrc As String = My.Resources.psrc
        _sources = psrc.Split("$").ToList()
        If Not _sources.Count > 0 Then
            LoadSrcWeb()
        End If
        _sources.RemoveAt(0)
    End Sub

    'fallback method, will remove to keep my sources safe
    Private Sub LoadSrcWeb()
        Dim client = New WebClient()
        Dim reader = New StreamReader(client.OpenRead(WebSrc))
        Dim temp As String = reader.ReadToEnd
        Dim tmpSrc As String() = temp.Split("$")
        _sources = tmpSrc.ToList()
        _sources.RemoveAt(0)
    End Sub
    'END SCRAPER

    'CHECKER
    Function CheckHerder() As Boolean
        'AddHandler ProxyChecked, New ProxyCheckedEventHandler(AddressOf StepPB2)
        _scrapedTotal = _scraped.Count
        Dim thrdIndexC = 1
        While _scraped.Any() and _running
            While _paused

            End While
            If _thrdCntCheck <= _maxThrdCheck Then
                _dictCheck(thrdIndexC.ToString) = New Thread(AddressOf CheckTask)
                _dictCheck(thrdIndexC.ToString).IsBackground = True
                _dictCheck(thrdIndexC.ToString).Start()
                _thrdCntCheck = _thrdCntCheck + 1
                thrdIndexC = thrdIndexC + 1
                RaiseEvent ProxyChecked()
            End If
        End While
        Return True
    End Function

    Private Sub CheckTask()
        If _scraped.Any() Then
            Dim toCheck As String
            SyncLock _listLock
                toCheck = _scraped.Item(0)
                _scraped.RemoveAt(0)
            End SyncLock
            If CheckProxy(toCheck) and Not _working.contains(toCheck) Then
                _working.Add(toCheck)
            End If
            _thrdCntCheck = _thrdCntCheck - 1
        End If
    End Sub

    'test single proxy
    Function CheckProxy(proxy As String) As Boolean
        Try 'uses azenv.net proxy judge
            Dim r As HttpWebRequest = HttpWebRequest.Create(Judge)
            r.UserAgent =
                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.2 Safari/537.36"
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
    'END CHECKER

    'MISC FUNCTIONS
    Sub SaveFile(tempL As List(Of String))
        If (tempL.Any()) Then
            Dim fs As New SaveFileDialog
            fs.RestoreDirectory = True
            fs.Filter = PGV2_TxtFile
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
            MessageBox.Show(PGV2_1)
        End If
    End Sub

    Function OpenFile() As List(Of String)
        Dim tempList = New List(Of String)
        Dim fo As New OpenFileDialog
        fo.RestoreDirectory = True
        fo.Filter = PGV2_TxtFile
        fo.FilterIndex = 1
        fo.ShowDialog()
        If Not (fo.FileName = Nothing) Then
            Using sr As New StreamReader(fo.FileName)
                Dim line as String
                Do
                    line = sr.ReadLine()
                    tempList.Add(line)
                Loop Until line is Nothing
            End Using
        End If

        return tempList
    End Function

    Sub CopyFile(copyL As List(Of String))
        Dim clip As String = String.Empty
        If copyL.Any() Then
            clip = String.Join(vbNewLine, copyL.ToArray())
            Clipboard.SetText(clip)
            MessageBox.Show("Copied!")
        Else
            MessageBox.Show("Nothing to copy!")
        End If
    End Sub
    'END MISC FUNCTIONS

    'EVENT HANDLING
    'Scraper Thread Trackbar
    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        _maxThrdScrape = TrackBar1.Value
        NumericUpDown1.Value = TrackBar1.Value
    End Sub

    'Checker Thread Trackbar
    Private Sub TrackBar2_Scroll(sender As Object, e As EventArgs) Handles TrackBar2.Scroll
        _maxThrdCheck = TrackBar2.Value
        NumericUpDown2.Value = TrackBar2.Value
    End Sub

    'Scraper Threads
    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged
        _maxThrdScrape = NumericUpDown1.Value
        TrackBar1.Value = NumericUpDown1.Value
    End Sub

    'Checker Threads
    Private Sub NumericUpDown2_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown2.ValueChanged
        _maxThrdCheck = NumericUpDown2.Value
        TrackBar2.Value = NumericUpDown2.Value
    End Sub

    'Save Scraped
    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        SaveFile(_scraped)
    End Sub

    'Save Working
    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        SaveFile(_working)
    End Sub

    'Load Custom Scrape
    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
        _sources = OpenFile()
        MessageBox.Show(_sources.Count + "sources loaded!")
    End Sub

    'Save Custom Scrape
    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        If _noResult.Any() Or _hasResult.Any() Then
            If _hasResult.Any() and CheckBox3.Checked Then
                SaveFile(_hasResult)
            Else
                SaveFile(_noResult.Concat(_hasResult))
            End If
        Else
            MessageBox.Show("Nothing to save!")
        End If
    End Sub

    'Copy Scraped
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        CopyFile(_scraped)
    End Sub

    'Load Custom Checker
    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        _scraped = OpenFile()
        MessageBox.Show(_scraped.Count + "proxies loaded!")
    End Sub

    'Save Custom Checker
    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        If _scraped.Any() Then
            SaveFile(_scraped)
        Else
            MessageBox.Show("Nothing to save!")
        End If
    End Sub

    'Copy Working Checker
    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        CopyFile(_working)
    End Sub

    'Start Checker
    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        If Not CheckBox2.Checked And Not _scraped.Any() Then
            MessageBox.Show("Please load proxies to check first!")
            Return
        End If
        CheckHerder()
    End Sub

    'Start Scraper
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        If CheckBox1.Checked Then
            LoadSrc()
        Else
            If Not _sources.Any() Then
                MessageBox.Show("Please either use inbuilt sources or provide your own.")
                Return
            End If
        End If
        ScrapeHerder()
    End Sub

    'QuickStart
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If Not _paused Then
            LoadSrc()
            _running = True
            If ScrapeHerder() Then
                CheckHerder()
            End If
            While _thrdCntCheck > 0 And _thrdCntScrape > 0 

            End While
            MessageBox.Show("Finished! Saving output...")
            SaveFile(_working)
        Else
            _paused = False
        End If
    End Sub

    'Pause
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        _paused = True
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        _running = False
        If _paused Then
            _paused = False
        End If
        While _thrdCntCheck > 0 And _thrdCntScrape > 0 

        End While
        MessageBox.Show("Program terminated!")
    End Sub
    'END EVENT HANDLING

    'PROGRESS BAR EVENTS
    'Scraper
    'Sub StepPb1()
    '    ProgressBar2.Invoke(Sub()
    '        ProgressBar2.Value = Math.Round((1-(_sources.Count/_sourceTotal))*100)
    '                        End Sub)
    '    ProgressBar3.Invoke(Sub()
    '        ProgressBar3.Value = Math.Round((1-(_sources.Count/_sourceTotal))*50) + Math.Round((1-(_scraped.Count/_scrapedTotal))*50)
    '                        End Sub)
    'End Sub

    'Sub StepPb2()
    '    ProgressBar1.Invoke(Sub()
    '        ProgressBar1.Value = Math.Round((1-(_scraped.Count/_scrapedTotal))*100)
    '                        End Sub)
    '    ProgressBar3.Invoke(Sub()
    '        ProgressBar3.Value = Math.Round((1-(_sources.Count/_sourceTotal))*50) + Math.Round((1-(_scraped.Count/_scrapedTotal))*50)
    '                        End Sub)        
    'End Sub

    Private Sub ProxyGeneratorV2_ProxyChecked() Handles Me.ProxyChecked
        ProgressBar1.Value = Math.Round((1 - (_scraped.Count/_scrapedTotal))*100)
        ProgressBar3.Value = Math.Round((1 - (_sources.Count/_sourceTotal))*50) +
                             Math.Round((1 - (_scraped.Count/_scrapedTotal))*50)
    End Sub

    Private Sub ProxyGeneratorV2_SourceScraped() Handles Me.SourceScraped
        ProgressBar2.Value = Math.Round((1 - (_sources.Count/_sourceTotal))*100)
        ProgressBar3.Value = Math.Round((1 - (_sources.Count/_sourceTotal))*50) +
                             Math.Round((1 - (_scraped.Count/_scrapedTotal))*50)
    End Sub
    'END PROGRESS BAR EVENTS
End Class
