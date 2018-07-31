' ===============================================================================
' This file is part of Ecopath with Ecosim (EwE)
'
' EwE is free software: you can redistribute it and/or modify it under the terms
' of the GNU General Public License version 2 as published by the Free Software 
' Foundation.
'
' EwE is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
' without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR 
' PURPOSE. See the GNU General Public License for more details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http://www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 1991- 
'    UBC Centre for the Oceans and Fisheries, Vancouver BC, Canada, and 
'    Ecopath International Initiative, Barcelona, Spain
'
' Stepwise Fitting Procedure by Sheila Heymans, Erin Scott, Jeroen Steenbeek
' Copyright 2015- Scottish Association for Marine Science, Oban, Scotland
'
' Erin Scott was funded by the Scottish Informatics and Computer Science
' Alliance (SICSA) Postgraduate Industry Internship Programme.
' ===============================================================================
'
#Region " Imports "

Option Strict On

Imports EwECore
Imports EwECore.FitToTimeSeries

#End Region ' Imports

Public Class cSFPEcosimRun
    Inherits cSFPGenericIterations
    Implements ISFPIterations

    Public Sub New(ByVal BOrF As Boolean)
        BaseorFish = BOrF
    End Sub

#Region " Implements ISFPIterations "

    Public Overrides Function Load() As Boolean

        Dim BSuccess As Boolean = False

        'Enable specific time series for Baseline or Fishing
        If MyBase.EnableTimeSeries() Then
            'Reset vunerabilities
            If MyBase.ResetVs() And MyBase.ResetFF() Then
                'Run a sensitivity of SS to V search for baseline
                If MyBase.RunSensitivityOfSSToV() Then
                    BSuccess = True
                End If
            End If
        End If

        Return BSuccess

    End Function

    Public Overrides Function Run() As Boolean
        Return MyBase.Run()
    End Function

#End Region

End Class
