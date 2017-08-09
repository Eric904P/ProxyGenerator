Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Threading
Imports ProxyGenerator_V2.My.Resources

Public Class ProxyGenerator_V2
    Private _thrdCntScrape = 0
    Private _maxThrdScrape = TrackBar1.Value
    Private _thrdCntCheck = 0
    Private _maxThrdCheck = TrackBar2.Value
    Private _scrapedTotal = 0
    Private _sourceTotal = 0
    Private _sources = New List(Of String)
    Private _scraped = New List(Of String)
    Private _working = New List(Of String)
    Private _noResult = New List(Of String)
    Private _listLock = New Object
    Private _dictScrape = New Dictionary(Of String, Thread)() 
    Private _dictCheck = New Dictionary(Of String, Thread)()
    Event ProxyWorking
    Event SourceFound
    Event ProxyChecked

    'SCRAPER
    Function ScrapeHerder() As Boolean
        _sourceTotal = _sources.Count
        Dim thrdIndex = 1
        While _sources.Count > 0
            If _thrdCntScrape <= _maxThrdScrape Then
                _dictScrape(thrdIndex.ToString) = New Thread(AddressOf ScrapeTask)
                _dictScrape(thrdIndex.ToString).IsBackground = True
                _dictScrape(thrdIndex.ToString).Start()
                _thrdCntScrape = _thrdCntScrape + 1
                thrdIndex = thrdIndex + 1
            End If
        End While
        Return True
    End Function

    Private Sub ScrapeTask()
        If _sources.Count > 0 Then
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
        Dim temp As String
        Try 'gets the entire web page as a string
            Dim r As HttpWebRequest = HttpWebRequest.Create(link)
            r.UserAgent =
                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
            r.Timeout = 15000
            Dim re As HttpWebResponse = r.GetResponse()
            Dim rs As Stream = re.GetResponseStream
            Using sr As New StreamReader(rs)
                temp = sr.ReadToEnd()
            End Using
            Dim src = temp
            rs.Dispose()
            rs.Close()
            r.Abort()
            'uses extractProx method to find all proxies on the webpage
            proxies = ExtractProx(src)

            If proxies.Count = 0 Then
                
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
        _scrapedTotal = _scraped.Count
        Dim thrdIndexC = 1
        While _scraped.Count > 0
            If _thrdCntCheck <= _maxThrdCheck Then
                _dictCheck(thrdIndexC.ToString) = New Thread(AddressOf CheckTask)
                _dictCheck(thrdIndexC.ToString).IsBackground = True
                _dictCheck(thrdIndexC.ToString).Start()
                _thrdCntCheck = _thrdCntCheck + 1
                thrdIndexC = thrdIndexC + 1
            End If
        End While
        Return True
    End Function

    Private Sub CheckTask()
        If _scraped.Count > 0 Then
            Dim toCheck As String
            SyncLock _listLock
                toCheck = _scraped.Item(0)
                _scraped.RemoveAt(0)
            End SyncLock
            If CheckProxy(toCheck) and Not _working.contains(toCheck) Then
                _working.Add(toCheck)
                Console.WriteLine(toCheck)
            End If
            _thrdCntCheck = _thrdCntCheck - 1
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
    'END MISC FUNCTIONS

    'EVENT HANDLING
    Private Sub TrackBar1_Scroll(sender As Object, e As EventArgs) Handles TrackBar1.Scroll
        _maxThrdScrape = TrackBar1.Value
        NumericUpDown1.Value = TrackBar1.Value
    End Sub

    Private Sub TrackBar2_Scroll(sender As Object, e As EventArgs) Handles TrackBar2.Scroll
        _maxThrdCheck = TrackBar2.Value
        NumericUpDown2.Value = TrackBar2.Value
    End Sub

    Private Sub NumericUpDown1_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown1.ValueChanged
        _maxThrdScrape = NumericUpDown1.Value
        TrackBar1.Value = NumericUpDown1.Value
    End Sub

    Private Sub NumericUpDown2_ValueChanged(sender As Object, e As EventArgs) Handles NumericUpDown2.ValueChanged
        _maxThrdCheck = NumericUpDown2.Value
        TrackBar2.Value = NumericUpDown2.Value
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        SaveFile(_scraped)
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        SaveFile(_working)
    End Sub
    'END EVENT HANDLING
End Class
