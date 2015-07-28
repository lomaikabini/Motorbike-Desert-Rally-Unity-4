// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// http://www.fluffyunderware.com
// =====================================================================
// CURVY 1.51
// =====================================================================
Curvy home: http://fluffyunderware.com/pages/unity-plugins/Curvy.php
Support forum: http://forum.fluffyunderware.com
Online manual: http://docs.fluffyunderware.com/curvy

====================== GETTING STARTED =============================================
1.) Create a curve with GameObject->Create Other->Curvy->Spline
2.) Attach SplineWalker to a GameObject (e.g. a simple cube), set it's Spline parameter to the curve
3.) Hit Run and enjoy!
4.) Read the docs and enjoy it even more!

====================== EXAMPLE SCENES ==============================================
Please see http://docs.fluffyunderware.com/curvy/examples for a detailed description of the provided example scenes!

====================== PACKAGE CONTENT =============================================
Assets/Curvy/Editor					Curvy Editor Scripts
Assets/Curvy/Base					Curvy Runtime Scripts
Assets/Curvy/Examples				Example scenes and scripts
Assets/Curvy/SplineWalker			Animates a transform over a curve
Assets/Curvy/SplineWalkerDistance	Animates a transform over a curve using absolute positions
Assets/Curvy/SplineWalkerCon		Animates a transform over a curve using connections
Assets/Curvy/SplineAlign			Aligns an object to a curve
Assets/Curvy/SplineShaper			SplineShaper component
Assets/Curvy/SplinePathCloneBuilder Manages clones of Transforms along a curve (can be created from a curve's inspector)
Assets/Curvy/SplinePathMeshBuilder	Extrudes a mesh along a curve (can be created from a curve's inspector)
Assets/Curvy/GLCurvyRenderer		Render a curve using GL.Draw - useful for debugging

====================== COLLABORATION WITH OTHER PACKAGES============================
Curvy supports the following packages:
- Magical Box: Spline Emitter, several Curvy related particle parameters
- playMaker: Curvy Custom Actions

Download them at http://www.fluffyunderware.com/pages/unity-plugins/curvy/addons.php

*** If you're an extension author and interested in Curvy integration, feel free to contact us ***
====================== HISTORY =====================================================
v1.51
	New:
		Added CurvySplineGroup component
		Added class CurvySplineBase as base class for CurvySpline and CurvySplineGroup. Almost all functionality now working on Groups as well
		Added CurvyInterpolation.Bezier, including full handle control
		Added Shortcut Key Bindings to Preferences
		Added custom Transform Handle that takes Constraints into account
		Added options to split, join and flip splines
		Added option to clone another spline's settings to CurvySpline.Create()
		New Control Points in the scene view will be created at mouse position if not inserted within an existing segment
		Added CurvySplineBase.MoveByLengthFast() to move by distance using actual length calculations instead of extrapolating
		Added CurvySplineSegment.IsFirstControlPoint() / CurvySplineSegment.IsLastControlPoint()
		Added option to show labels in the Scene View
		Added option to SplineMeshBuilder to calculate tangents
		Added new examples: EndlessRunner, JunctionWalker,SplineGroup
		Added CurvyShaper (Superformula)
		Added an option to CurvySpline.GetNearestPointTF() to accept a list of segments to check
		Added CurvySplineBase.ExtrapolateDistanceToTF()/CurvySplineBase.ExtrapolateDistanceToTFFast()
		Added Forward option to SplineWalker and SplineWalkerDistance to set movement direction
		Code now Windows Store compliant (untested)
	Changes:
		CurvySplineBase.Refresh() now refreshes on the next call to Update(). Use RefreshImmediately() for instant refresh
		Renamed OnRefreshEvent to RefreshEvent
		Changed SplinePathCloneBuilder.Spline type to support CurvySplineBase
		Changed SplinePathMeshBuilder.Spline type to support CurvySplineBase
		Closing the Costraint Window will now deactivate any constraints
		Reworked Control Point Gizmo Scaling
		CurvySplineBase.Interpolate() now accepts an additional interpolation method
		Changed examples folder structure to better categorize the examples
		CurvySplineBase.GetExtrusionPoint() simplified. Flagged the old syntax as obsolete (will be removed in the next version)
		SplineWalker and SplineWalkerFixed don't set initial direction by the sign of speed anymore.
	Fixes:
		Shorcuts working when Control Point is selected in the hierarchy window. Finally!
		Fixed several methods (mostly movement based) not dealing with large values correctly
		Fixed some bugs and possible runtime errors in SplineMeshBuilder
		Fixed UserValue array size could be set to a negative value
		Several small bugfixes
		Several stability and performance optimizations
v1.50
	New:
		Added SplinePathCloneBuilder script to dynamically clone transforms along a spline 
		Added SplinePathMeshBuilder script to dynamically create a mesh representation
		Added Export wizard to export closed splines as meshes
		Added SceneView shortcuts for adding (G/Shift-G), deleting(H) and toggling Control Points (T/Shift-T)
		Added option to let a Control Point become the first Control Point
		Added option to CurvySpline.Add() to insert before (also available in the Control Point's scene GUI)
		Added option to CurvySpline.MoveBy() and CurvySpline.MoveByFast() to change accuracy
		Added option to GetApproximation() to return local coordinates
		Added CurvySpline.GetBounds()
		Added CurvySpline.MoveByAngle() and CurvySpline.MoveByAngleFast() to move until a certain curvation angle is reached
		Added CurvySpline.OnRefresh event that will be raised when the spline updates
		Added CurvySpline.InterpolateScale() and CurvySplineSegment.InterpolateScale()
		Added CurvySpline.Destroy()
	Changes:
		Removed RailRunner example
		Improved inspector look&feel
		Reworked GameObject->Create Other->Curvy menu structure into a subfolder to make room for future components
		Added proper Undo support for all scene view operations
		Reworked Constraint wizard
	Fixes:
		Fixed TCB properties bug when multi-editing Control Points
		Fixed distance to f returning 0.9999999 instead of 1 due to float inaccuracy
		Fixed some crash cases when recompiling during runtime
		Fixed broken AlignWizard example
		Fixed a bug sometimes causing weird spline orientation when entering runtime
		Fixed several minor bugs
v1.04
	New:
		Added CurvySplineSegment.NextControlPoint and CurvySplineSegment.PreviousControlPoint
		Added CurvySplineSegment.IsFirstSegment and CurvySplineSegment.IsLastSegment
		Added CurvySpline.Swirl to swirl orientation vector by several modes
		Added option to show tangent gizmos
		Added option to show UserValues in the scene view
		Added CurvySplineSegment.SmoothEdgeTangent (results in orientation lerp between corners)
		Added API for better playMaker access: CurvySplineSegment.SegmentIndex, CurvySplineSegment.ControlPointIndex, CurvySpline.Segments
		Added Rails example scene
		Improved documentation
	Changes:
		Changed Orientation calculation to Parallel Transport Framing, making it much more stable
		Tangent->ControlPoint orientation: First Up-Vector now keeps orthogonal
		Huge performance increase of Distance based methods
		Reworked Performance example
	Fixes:
		Several fixes regarding caching
		Fixed Tangent calculation
		Fixed error when using GetNearestTF on an empty spline
v1.02/1.03
	New:
		Added SplineWalkerDistance script - can be useful when working with dynamic splines
		Added CurvySpline.ReloadControlPoints()
		Added new example scene: DynamicSpline
	Changes:
		Improved SplineWalker script
		Control points now will be numbered consecutively when adding/deleting control points
	Fixes:
		- CurvySpline.GetApproximation(), CurvySpline.GetApproximationT() and CurvySpline.GetApproximationUpVectors() returning wrong values
		- CurvySplineAlignWizard now uses ArrowCap instead of DrawArrow
v1.01
	New:
		Added UserValues array to Control Points, to be used with InterpolateUserValue()
		Added editor wizard to align objects to a spline
		SplineAlign script to just align position and rotation of objects both in editor and at runtime
	Changes:
		SplineWalker now continously sets initial position and rotation in the editor
	Fixes:
		- Up-Vectors of closed splines not smoothed out when using Tangent Orientation
v1.00 Initial release