Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Threading

Public Class Form1
    Private _sources As List(Of String) = New List(Of String)
    Private _scraped As List(Of String) = New List(Of String)
    Private _working As List(Of String) = New List(Of String)
    Private _running As Boolean = False
    Private _thrd = new Thread(AddressOf Start)

#Region "MAIN FUNCTIONS"
    Private Function Start() As Boolean
        if _running Then
            _thrd.Stop()
            Return False
        End If
        LoadSrc()
        UpdateLabel(1,_sources.Count)
        dim sources = _sources.Count
        _running = true
        For Each link As String in _sources
            If not _running Then
                _thrd.Stop()
                Return false
            End If 
            _scraped.AddRange(ScrapeLink(link))
            sources = sources - 1
            UpdateLabel(1,sources)
            UpdateLabel(2,_scraped.Count)
        Next
        For Each proxy As String in _scraped
            If not _running Then
                _thrd.Stop()
                Return false
            End If
            If CheckProxy(proxy) Then
                _working.Add(proxy)
                AppendText(proxy)
                UpdateLabel(3,_working.Count)
            End If
        Next
        Return True
    End Function

    Private Sub UpdateLabel(num As Integer, count As Integer)
        select case num
            Case 1
                Label1.Invoke(Sub()
                    Label1.Text = "Sources: " + count.ToString()
                    Label1.Update()
                End Sub)
            Case 2
                Label2.Invoke(Sub()
                    Label2.Text = "Scraped: " + count.ToString()
                    Label2.Update()
                End Sub)
            Case 3
                Label3.Invoke(Sub()
                    Label3.Text = "Working: " + count.ToString()
                    Label3.Update()
                End Sub)
        End Select
    End Sub

    Private Sub AppendText(s As String)
        TextBox1.Invoke(Sub()
            Dim old As String = TextBox1.Text
            TextBox1.Text = old + s
            TextBox1.Update()
         End Sub) 
    End Sub
#End Region

#Region "SCRAPER"
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
                AppendText(proxies.Count + " || " + link)
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
        Dim reader = New StreamReader(client.OpenRead("https://pastebin.com/raw/KhiFJE4m"))
        Dim temp As String = reader.ReadToEnd
        Dim tmpSrc As String() = temp.Split("$")
        _sources = tmpSrc.ToList()
        _sources.RemoveAt(0)
    End Sub
#End Region

#Region "CHECKER"
    'test single proxy
    Function CheckProxy(proxy As String) As Boolean
        Try 'uses azenv.net proxy judge
            Dim r As HttpWebRequest = HttpWebRequest.Create("http://azenv.net")
            r.UserAgent =
                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.2 Safari/537.36"
            r.Timeout = 3000
            r.ReadWriteTimeout = 3000
            r.Proxy = New WebProxy(proxy)
            Using sr As New StreamReader(r.GetResponse().GetResponseStream())
                If sr.ReadToEnd().Contains("HTTP_HOST = azenv.net") Then
                    r.Abort()
                    File.AppendAllLines(workingPath, {proxy} )
                    Return True
                End If
            End Using
            r.Abort()
        Catch ex As Exception
            Return False
        End Try
        Return False
    End Function
#End Region

#Region "MISC"
    Sub WriteToFile(text As List(Of String), filePath As String)
        File.AppendAllLines(filePath,text)
    End Sub

    Sub SaveFile(tempL As List(Of String))
        If (tempL.Any()) Then
            Dim fs As New SaveFileDialog
            fs.RestoreDirectory = True
            fs.Filter = "txt files (*.txt)|*.txt"
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
            MessageBox.Show("Nothing to save!")
        End If
    End Sub

    Function OpenFile() As List(Of String)
        Dim tempList = New List(Of String)
        Dim fo As New OpenFileDialog
        fo.RestoreDirectory = True
        fo.Filter = "txt files (*.txt)|*.txt"
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

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Button2.Text = "Stop"
        _thrd.IsBackground = True
        _thrd.Start()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If not _running then
            _scraped.Clear()
            _working.Clear()
            _sources.Clear()
            TextBox1.Text = "Cleared!"
        Else 
            _running = false
            Button2.Text = "Clear"
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        SaveFile(_working)
    End Sub
#End Region
End Class
