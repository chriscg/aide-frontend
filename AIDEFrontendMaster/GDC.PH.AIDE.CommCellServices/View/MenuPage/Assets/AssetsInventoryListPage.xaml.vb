﻿Imports UI_AIDE_CommCellServices.ServiceReference1
Imports System.Reflection
Imports System.IO
Imports System.Diagnostics
Imports System.ServiceModel
Imports System.Collections.ObjectModel
Imports System.Windows.Xps.Packaging
Imports System.Windows.Xps
Imports System.Printing
Imports System.Drawing.Printing

<CallbackBehavior(ConcurrencyMode:=ConcurrencyMode.Single, UseSynchronizationContext:=False)>
Public Class AssetsInventoryListPage
    Implements ServiceReference1.IAideServiceCallback

#Region "FIELDS"
    Private frame As Frame
    Private profile As New Profile
    Private page As String
    Private _addframe As New Frame
    Private _menugrid As Grid
    Private _submenuframe As Frame

    Private _AideService As ServiceReference1.AideServiceClient
    Dim lstAssets As Assets()
    Dim show As Boolean = True

    Private Enum PagingMode
        _First = 1
        _Next = 2
        _Previous = 3
        _Last = 4
    End Enum

#End Region

#Region "Paging Declarations"
    Dim startRowIndex As Integer
    Dim lastRowIndex As Integer
    Dim pagingPageIndex As Integer
    Dim pagingRecordPerPage As Integer = 10

#End Region

#Region "CONSTRUCTOR"

    Public Sub New(_frame As Frame, _profile As Profile, addframe As Frame, menugrid As Grid, submenuframe As Frame, page As String)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        frame = _frame
        Me.profile = _profile
        Me._addframe = addframe
        Me._menugrid = menugrid
        Me._submenuframe = submenuframe
        SetUnApprovedtTabVisible()
        SetData()
        If page = "Personal" Then
            SR.SelectedIndex = 0
        ElseIf page = "Update" Then
            SR.SelectedIndex = 1
        ElseIf page = "Approval" Then
            SR.SelectedIndex = 2
        End If

    End Sub
#End Region

#Region "EVENTS"

    Private Sub btnExport_Click(sender As Object, e As RoutedEventArgs) Handles btnExport.Click
        Try
            Dim path As String = "C:\Program Files (x86)\GDC PH\AIDE CommCell\Excel Files"
            Dim exists As Boolean = Directory.Exists(path)

            If Not exists Then Directory.CreateDirectory(path)
            show = False
            lv_assetInventoryList.SelectAllCells()
            lv_assetInventoryList.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader
            ApplicationCommands.Copy.Execute(Nothing, lv_assetInventoryList)
            Dim resultat As String = CStr(Clipboard.GetData(DataFormats.CommaSeparatedValue))
            Dim result As String = CStr(Clipboard.GetData(DataFormats.Text))
            lv_assetInventoryList.UnselectAllCells()
            Dim file1 As StreamWriter = New System.IO.StreamWriter("C:\Program Files (x86)\GDC PH\AIDE CommCell\Excel Files\Assets Inventory.xls")
            file1.WriteLine(result.Replace(","c, " "c))
            file1.Close()
            MessageBox.Show("Exporting DataGrid data to Excel file created.xls")
            show = True
            'frame.Navigate(New AssetsInventoryListPage(frame, profile, _addframe, _menugrid, _submenuframe))
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "AIDE")
        End Try
    End Sub

    Private Sub SR_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles SR.SelectionChanged
        e.Handled = True
        Me.DataContext = Nothing
        SetData()
        If SR.SelectedIndex = 0 Then
            page = "Personal"
        ElseIf SR.SelectedIndex = 1 Then
            btnPrint.Visibility = Windows.Visibility.Visible
            page = "All"
        Else
            page = "Approval"
        End If
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtSearch.TextChanged
        If txtSearch.Text = String.Empty Then
            SetData()
        Else
            SetDataForSearch(txtSearch.Text)
        End If
        e.Handled = True
    End Sub

    Private Sub lv_assetInventoryListOwn_LoadingRow(sender As Object, e As DataGridRowEventArgs) Handles lv_assetInventoryListOwn.LoadingRow
        Dim RowDataContaxt As AssetsModel = TryCast(e.Row.DataContext, AssetsModel)

        If RowDataContaxt IsNot Nothing Then
            If RowDataContaxt.EMP_ID = profile.Emp_ID And RowDataContaxt.STATUS = 2 And RowDataContaxt.APPROVAL = 1 Then
                e.Row.Background = New BrushConverter().ConvertFrom("#CCFFD8D8")
                e.Row.Foreground = New SolidColorBrush(Colors.Black)
            End If
        End If
    End Sub

    Private Sub lv_assetInventoryListOwn_MouseDoubleClick(sender As Object, e As SelectionChangedEventArgs) Handles lv_assetInventoryListOwn.SelectionChanged
        e.Handled = True
        If lv_assetInventoryListOwn.SelectedIndex <> -1 Then

            If lv_assetInventoryListOwn.SelectedItem IsNot Nothing Then
                Dim assetsModel As New AssetsModel

                assetsModel.ASSET_ID = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).ASSET_ID
                assetsModel.EMP_ID = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).EMP_ID
                assetsModel.ASSET_DESC = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).ASSET_DESC
                assetsModel.MANUFACTURER = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).MANUFACTURER
                assetsModel.MODEL_NO = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).MODEL_NO
                assetsModel.SERIAL_NO = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).SERIAL_NO
                assetsModel.ASSET_TAG = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).ASSET_TAG
                assetsModel.DATE_ASSIGNED = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).DATE_ASSIGNED
                assetsModel.STATUS = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).STATUS
                assetsModel.OTHER_INFO = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).OTHER_INFO
                assetsModel.FULL_NAME = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).FULL_NAME
                assetsModel.ISAPPROVED = CType(lv_assetInventoryListOwn.SelectedItem, AssetsModel).ISAPPROVED
                _addframe.Navigate(New AssetsInventoryAddPage(assetsModel, frame, profile, "Personal", _addframe, _menugrid, _submenuframe))
                frame.IsEnabled = False
                frame.Opacity = 0.3
                _menugrid.IsEnabled = False
                _menugrid.Opacity = 0.3
                _submenuframe.IsEnabled = False
                _submenuframe.Opacity = 0.3
                _addframe.Margin = New Thickness(150, 50, 150, 50)
                _addframe.Visibility = Visibility.Visible
            End If
        End If

    End Sub

    Private Sub lv_assetInventoryList_MouseDoubleClick(sender As Object, e As SelectionChangedEventArgs) Handles lv_assetInventoryList.SelectionChanged
        e.Handled = True
        If show = True Then
            If lv_assetInventoryList.SelectedIndex <> -1 Then
                If lv_assetInventoryList.SelectedItem IsNot Nothing Then
                    If CType(lv_assetInventoryList.SelectedItem, AssetsModel).STATUS <> 1 And CType(lv_assetInventoryList.SelectedItem, AssetsModel).EMP_ID <> profile.Emp_ID And profile.Permission <> "Manager" Then
                        Exit Sub
                    Else
                        Dim assetsModel As New AssetsModel

                        assetsModel.ASSET_ID = CType(lv_assetInventoryList.SelectedItem, AssetsModel).ASSET_ID
                        assetsModel.EMP_ID = CType(lv_assetInventoryList.SelectedItem, AssetsModel).EMP_ID
                        assetsModel.ASSET_DESC = CType(lv_assetInventoryList.SelectedItem, AssetsModel).ASSET_DESC
                        assetsModel.MANUFACTURER = CType(lv_assetInventoryList.SelectedItem, AssetsModel).MANUFACTURER
                        assetsModel.MODEL_NO = CType(lv_assetInventoryList.SelectedItem, AssetsModel).MODEL_NO
                        assetsModel.SERIAL_NO = CType(lv_assetInventoryList.SelectedItem, AssetsModel).SERIAL_NO
                        assetsModel.ASSET_TAG = CType(lv_assetInventoryList.SelectedItem, AssetsModel).ASSET_TAG
                        assetsModel.DATE_ASSIGNED = CType(lv_assetInventoryList.SelectedItem, AssetsModel).DATE_ASSIGNED
                        assetsModel.STATUS = CType(lv_assetInventoryList.SelectedItem, AssetsModel).STATUS
                        assetsModel.OTHER_INFO = CType(lv_assetInventoryList.SelectedItem, AssetsModel).OTHER_INFO
                        assetsModel.FULL_NAME = CType(lv_assetInventoryList.SelectedItem, AssetsModel).FULL_NAME
                        _addframe.Navigate(New AssetsInventoryAddPage(assetsModel, frame, profile, "Update", _addframe, _menugrid, _submenuframe))
                        frame.IsEnabled = False
                        frame.Opacity = 0.3
                        _menugrid.IsEnabled = False
                        _menugrid.Opacity = 0.3
                        _submenuframe.IsEnabled = False
                        _submenuframe.Opacity = 0.3
                        _addframe.Margin = New Thickness(150, 50, 150, 50)
                        _addframe.Visibility = Visibility.Visible
                    End If
                End If
            End If
        End If
    End Sub

    Private Sub lv_assetInventoryListUnapproved_MouseDoubleClick(sender As Object, e As SelectionChangedEventArgs) Handles lv_assetInventoryListUnapproved.SelectionChanged
        e.Handled = True
        If lv_assetInventoryListUnapproved.SelectedIndex <> -1 Then

            If lv_assetInventoryListUnapproved.SelectedItem IsNot Nothing Then
                Dim assetsModel As New AssetsModel

                assetsModel.ASSET_ID = CType(lv_assetInventoryListUnapproved.SelectedItem, AssetsModel).ASSET_ID
                assetsModel.EMP_ID = CType(lv_assetInventoryListUnapproved.SelectedItem, AssetsModel).EMP_ID
                assetsModel.ASSET_DESC = CType(lv_assetInventoryListUnapproved.SelectedItem, AssetsModel).ASSET_DESC
                assetsModel.MANUFACTURER = CType(lv_assetInventoryListUnapproved.SelectedItem, AssetsModel).MANUFACTURER
                assetsModel.MODEL_NO = CType(lv_assetInventoryListUnapproved.SelectedItem, AssetsModel).MODEL_NO
                assetsModel.SERIAL_NO = CType(lv_assetInventoryListUnapproved.SelectedItem, AssetsModel).SERIAL_NO
                assetsModel.ASSET_TAG = CType(lv_assetInventoryListUnapproved.SelectedItem, AssetsModel).ASSET_TAG
                assetsModel.DATE_ASSIGNED = CType(lv_assetInventoryListUnapproved.SelectedItem, AssetsModel).DATE_ASSIGNED
                assetsModel.STATUS = CType(lv_assetInventoryListUnapproved.SelectedItem, AssetsModel).STATUS
                assetsModel.OTHER_INFO = CType(lv_assetInventoryListUnapproved.SelectedItem, AssetsModel).OTHER_INFO
                assetsModel.FULL_NAME = CType(lv_assetInventoryListUnapproved.SelectedItem, AssetsModel).FULL_NAME
                _addframe.Navigate(New AssetsInventoryAddPage(assetsModel, frame, profile, "Approval", _addframe, _menugrid, _submenuframe))
                frame.IsEnabled = False
                frame.Opacity = 0.3
                _menugrid.IsEnabled = False
                _menugrid.Opacity = 0.3
                _submenuframe.IsEnabled = False
                _submenuframe.Opacity = 0.3
                _addframe.Margin = New Thickness(150, 50, 150, 50)
                _addframe.Visibility = Visibility.Visible
                'frame.Navigate(New AssetsInventoryAddPage(assetsModel, frame, profile, "Approval"))
            End If
        End If

    End Sub

    Private Sub btnPrint_Click(sender As Object, e As RoutedEventArgs) Handles btnPrint.Click
        Dim dialog As PrintDialog = New PrintDialog()
        If lv_assetInventoryList.HasItems Then
            If CBool(dialog.ShowDialog().GetValueOrDefault()) Then
                dialog.PrintTicket.PageOrientation = PageOrientation.Landscape

                Dim pageSize As Size = New Size(dialog.PrintableAreaWidth, dialog.PrintableAreaHeight)
                lv_assetInventoryList.Measure(pageSize)
                lv_assetInventoryList.Arrange(New Rect(5, 5, pageSize.Width, pageSize.Height))
                dialog.PrintVisual(lv_assetInventoryList, "Print Asset Inventory")
            End If
        Else
            MsgBox("No Records Found!", MsgBoxStyle.Exclamation, "AIDE")
        End If
    End Sub
#End Region

#Region "PRIVATE FUNCTION"

    Public Sub SetUnApprovedtTabVisible()
        If profile.Permission = "Manager" Then
            Unapproved.Visibility = Windows.Visibility.Visible
        End If
    End Sub

    Public Sub SetData()
        Try
            If InitializeService() Then
                If SR.SelectedIndex = 0 Then
                    lstAssets = _AideService.GetMyAssets(profile.Emp_ID)
                    btnPrint.Visibility = Windows.Visibility.Hidden
                ElseIf SR.SelectedIndex = 1 Then
                    lstAssets = _AideService.GetAllAssetsInventoryByEmpID(profile.Emp_ID)
                    btnPrint.Visibility = Windows.Visibility.Hidden
                Else
                    lstAssets = _AideService.GetAllAssetsInventoryUnApproved(profile.Emp_ID)
                    btnPrint.Visibility = Windows.Visibility.Hidden
                End If

                SetPaging(PagingMode._First)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Public Sub LoadData()
        Try
            Dim lstAssetsList As New ObservableCollection(Of AssetsModel)
            Dim assetsDBProvider As New AssetsDBProvider
            Dim assetsVM As New AssetsViewModel()

            Dim objAssets As New Assets()

            ' Set the MyLessonLearntList 
            For i As Integer = startRowIndex To lastRowIndex
                objAssets = lstAssets(i)
                assetsDBProvider.SetAssetInventoryList(objAssets)
            Next

            For Each rawUser As MyAssets In assetsDBProvider.GetAssetInventoryList()
                lstAssetsList.Add(New AssetsModel(rawUser))
            Next

            assetsVM.AssetInventoryList = lstAssetsList
            Me.DataContext = assetsVM
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "FAILED")
        End Try
    End Sub

    Public Sub SetDataForSearch(input As String)
        Try
            If InitializeService() Then
                lstAssets = _AideService.GetAllAssetsInventoryBySearch(profile.Emp_ID, input, page)
                SetPaging(PagingMode._First)
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Public Function InitializeService() As Boolean
        Dim bInitialize As Boolean = False
        Try
            'DisplayText("Opening client service...")
            Dim Context As InstanceContext = New InstanceContext(Me)
            _AideService = New AideServiceClient(Context)
            _AideService.Open()
            bInitialize = True
            'DisplayText("Service opened successfully...")
            'Return True
        Catch ex As SystemException
            _AideService.Abort()
        End Try
        Return bInitialize
    End Function

    Private Sub SetPaging(mode As Integer)
        Try
            Dim totalRecords As Integer = lstAssets.Length

            Select Case mode
                Case CInt(PagingMode._Next)
                    ' Set the rows to be displayed if the total records is more than the (Record per Page * Page Index)
                    If totalRecords > (pagingPageIndex * pagingRecordPerPage) Then

                        ' Set the last row to be displayed if the total records is more than the (Record per Page * Page Index) + Record per Page
                        If totalRecords >= ((pagingPageIndex * pagingRecordPerPage) + pagingRecordPerPage) Then
                            lastRowIndex = ((pagingPageIndex * pagingRecordPerPage) + pagingRecordPerPage) - 1
                        Else
                            lastRowIndex = totalRecords - 1
                        End If

                        startRowIndex = pagingPageIndex * pagingRecordPerPage
                        pagingPageIndex += 1
                    Else
                        startRowIndex = (pagingPageIndex - 1) * pagingRecordPerPage
                        lastRowIndex = totalRecords - 1
                    End If
                    ' Bind data to the Data Grid
                    LoadData()
                    Exit Select
                Case CInt(PagingMode._Previous)
                    ' Set the Previous Page if the page index is greater than 1
                    If pagingPageIndex > 1 Then
                        pagingPageIndex -= 1

                        startRowIndex = ((pagingPageIndex * pagingRecordPerPage) - pagingRecordPerPage)
                        lastRowIndex = (pagingPageIndex * pagingRecordPerPage) - 1
                        LoadData()
                    End If
                    Exit Select
                Case CInt(PagingMode._First)
                    If totalRecords > pagingRecordPerPage Then
                        pagingPageIndex = 2
                        SetPaging(CInt(PagingMode._Previous))
                    Else
                        pagingPageIndex = 1
                        startRowIndex = ((pagingPageIndex * pagingRecordPerPage) - pagingRecordPerPage)

                        If Not totalRecords = 0 Then
                            lastRowIndex = totalRecords - 1
                            LoadData()
                        Else
                            lastRowIndex = 0
                            Me.DataContext = Nothing
                        End If

                    End If
                    Exit Select
                Case CInt(PagingMode._Last)
                    pagingPageIndex = (lstAssets.Length / pagingRecordPerPage)
                    SetPaging(CInt(PagingMode._Next))
                    Exit Select
            End Select

            DisplayPagingInfo()
        Catch ex As Exception
            MsgBox(ex.Message, MsgBoxStyle.Critical, "FAILED")
        End Try
    End Sub

    Private Sub DisplayPagingInfo()
        Dim pagingInfo As String

        ' If there has no data found
        If lstAssets.Length = 0 Then
            pagingInfo = "No Results Found "
            GUISettingsOff()
        Else
            pagingInfo = "Displaying " & startRowIndex + 1 & " to " & lastRowIndex + 1
            GUISettingsOn()
        End If

        'lblPagingInfo2.Content = pagingInfo

    End Sub

    Private Sub GUISettingsOff()
        lv_assetInventoryList.Visibility = Windows.Visibility.Hidden
 
        btnPrev2.IsEnabled = False
        btnNext2.IsEnabled = False
    End Sub

    Private Sub GUISettingsOn()
        lv_assetInventoryList.Visibility = Windows.Visibility.Visible

        btnPrev2.IsEnabled = True
        btnNext2.IsEnabled = True
    End Sub

    Private Sub btnNext_Click(sender As Object, e As RoutedEventArgs)
        SetPaging(CInt(PagingMode._Next))
    End Sub

    Private Sub btnPrev_Click(sender As Object, e As RoutedEventArgs)
        SetPaging(CInt(PagingMode._Previous))
    End Sub

    Private Sub btnFirst_Click(sender As Object, e As RoutedEventArgs)
        SetPaging(CInt(PagingMode._First))
    End Sub

    Private Sub btnLast_Click(sender As Object, e As RoutedEventArgs)
        SetPaging(CInt(PagingMode._Last))
    End Sub
#End Region

#Region "ICallBack Function"
    Public Sub NotifyError(message As String) Implements IAideServiceCallback.NotifyError
        If message <> String.Empty Then
            MessageBox.Show(message)
        End If
    End Sub

    Public Sub NotifyOffline(EmployeeName As String) Implements IAideServiceCallback.NotifyOffline

    End Sub

    Public Sub NotifyPresent(EmployeeName As String) Implements IAideServiceCallback.NotifyPresent

    End Sub

    Public Sub NotifySuccess(message As String) Implements IAideServiceCallback.NotifySuccess
        If message <> String.Empty Then
            MessageBox.Show(message)
        End If
    End Sub

    Public Sub NotifyUpdate(objData As Object) Implements IAideServiceCallback.NotifyUpdate

    End Sub
#End Region

End Class
