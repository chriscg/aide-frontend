﻿Imports UI_AIDE_CommCellServices.ServiceReference1

Class BillabilitySubMenu
    Private pFrame As New Frame
    Private profile As New Profile
    Private addframe As Frame
    Private menugrid As Grid
    Private submenuframe As Frame

    Public Sub New(_pFrame As Frame, _profile As Profile, _addframe As Frame, _menugrid As Grid, _submenuframe As Frame)
        pFrame = _pFrame
        profile = _profile
        addframe = _addframe
        menugrid = _menugrid
        submenuframe = _submenuframe
        InitializeComponent()
    End Sub

    Private Sub NonBillables_Click(sender As Object, e As RoutedEventArgs)
        pFrame.Navigate(New BillabilityPage(profile, pFrame))
    End Sub

    Private Sub VacationLeave_Click(sender As Object, e As RoutedEventArgs)
        pFrame.Navigate(New BillabilityVacationLeavePage(profile, pFrame))
    End Sub

    Private Sub SickLeave_Click(sender As Object, e As RoutedEventArgs)
        pFrame.Navigate(New BillabilitySickLeavePage(profile, pFrame))
    End Sub

    Private Sub Billables_Click(sender As Object, e As RoutedEventArgs)
        pFrame.Navigate(New BillablesPage(profile, pFrame))
    End Sub

    Private Sub Weekly_Click(sender As Object, e As RoutedEventArgs)
        pFrame.Navigate(New WeeklyReportPage(pFrame, profile.Emp_ID, profile.Email_Address, addframe, menugrid, submenuframe))
    End Sub

End Class
