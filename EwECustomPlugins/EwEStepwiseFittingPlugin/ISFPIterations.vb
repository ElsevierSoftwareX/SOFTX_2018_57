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
Imports System.Windows.Forms


#End Region ' Imports

''' ---------------------------------------------------------------------------
''' <summary>
''' 
''' </summary>
''' ---------------------------------------------------------------------------
Public Interface ISFPIterations

    ''' -----------------------------------------------------------------------
    ''' <summary>Enumerated type, defining possible iteration run state values.</summary>
    ''' -----------------------------------------------------------------------
    Enum eRunState As Integer
        ''' <summary>Iteration has not ran yet.</summary>
        Idle = 0
        ''' <summary>Iteration ran successfully.</summary>
        Completed
        ''' <summary>Iteration encountered an error while running.</summary>
        [Error]
        ''' <summary>Iteration running</summary>
        Running
    End Enum

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Initialize the iteration
    ''' </summary>
    ''' <param name="c"></param>
    ''' <param name="tsi"></param>
    ''' <param name="SSToVChoice"></param>
    ''' <param name="Params"></param>
    ''' -----------------------------------------------------------------------
    Sub Init(ByVal c As cCore, ByVal tsi As Integer, ByVal SSToVChoice As Boolean, ByVal Params As cSFPParameters, ByVal mFrm As Form)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Load the paratmeter configuration to Ecosim, eg make all the input parameter 
    ''' tweaks to Ecosim, Fit to TS, vunerability searches etc but do not run yet
    ''' </summary>
    ''' <returns>True if load successful</returns>
    ''' -----------------------------------------------------------------------
    Function Load() As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Run the Iteration
    ''' </summary>
    ''' <returns>True if run successful</returns>
    ''' -----------------------------------------------------------------------
    Function Run() As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Clean up after the run
    ''' Make sure Clear is ALWAYS called on any created iterations object after the object has been used
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Sub Clear()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="cSFPParameters"/> that the iteration can use.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Property Parameters As cSFPParameters

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the name of the hypothesis / iteration.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property Name As String

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the iteration type to be Baseline = true or Fishing = false
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Property BaseorFishValue As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the K value.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Property K As Integer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the EstimatedV value.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Property EstimatedV As Integer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the number of spline points.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Property SplinePoints As Integer

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the computed Sum of Squares.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property SS As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    '''  Get the computed Sum of Squares per <see cref="cTimeSeries.Index">time series</see>.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property TimeSeriesSS As Single()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the computed Akaike Information Criterion.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property AIC As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the computed Akaike Information Criterion with a correction for finite sample sizes.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    ReadOnly Property AICc As Single

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set if this iteration is allowed to run.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Property Enabled As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the <see cref="eRunState">run state</see> of an iteration.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Property RunState As eRunState

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set whether an iteration fitted best.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Property IsBestFit As Boolean

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the data for the anomaly shape for an iteration.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Function AnomalyShape() As Single()

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Returns the fitted vulnerabilities for an iteration.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Function Vulnerabilities() As Single(,)

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Apply computed anomaly shape data and vulnerabilities to EwE.
    ''' </summary>
    ''' <returns>True if successful.</returns>
    ''' -----------------------------------------------------------------------
    Function Apply() As Boolean

End Interface

