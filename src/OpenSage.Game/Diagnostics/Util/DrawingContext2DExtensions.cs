using System;
using System.Numerics;
using OpenSage.Graphics.Cameras;
using OpenSage.Gui;
using OpenSage.Mathematics;

namespace OpenSage.Diagnostics.Util;

internal static class DrawingContext2DExtensions
{
    public static void DrawSphere(this DrawingContext2D drawingContext, Camera camera, in SphereShape shape)
    {
        //DebugDrawAxisAlignedBoundingArea(drawingContext, camera);

        // Bounding Sphere
        const int sides = 8;
        var lineColor = new ColorRgbaF(220, 220, 220, 255);

        var firstPoint = Vector2.Zero;
        var previousPoint = Vector2.Zero;

        for (var i = 0; i < sides; i++)
        {
            var angle = 2 * MathF.PI * i / sides;
            var point = camera.WorldToScreenPoint(shape.Center + new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0) * shape.Radius);
            var screenPoint = point.Vector2XY();

            // No line gets drawn on the first iteration
            if (i == 0)
            {
                firstPoint = screenPoint;
                previousPoint = screenPoint;
                continue;
            }

            drawingContext.DrawLine(new Line2D(previousPoint, screenPoint), 1, lineColor);

            // If this is the last point, complete the circle
            if (i == sides - 1)
            {
                drawingContext.DrawLine(new Line2D(screenPoint, firstPoint), 1, lineColor);
            }

            previousPoint = screenPoint;

            var firstPoint2 = Vector2.Zero;
            var previousPoint2 = Vector2.Zero;
            for (var j = 0; j < sides; j++)
            {
                var angle2 = 2 * MathF.PI * j / sides;
                var point2 = camera.WorldToScreenPoint(shape.Center + new Vector3(MathF.Sin(angle2) * MathF.Cos(angle), MathF.Sin(angle2) * MathF.Sin(angle), MathF.Cos(angle2)) * shape.Radius);
                var screenPoint2 = point2.Vector2XY();

                // No line gets drawn on the first iteration
                if (j == 0)
                {
                    firstPoint2 = screenPoint2;
                    previousPoint2 = screenPoint2;
                    continue;
                }

                drawingContext.DrawLine(new Line2D(previousPoint2, screenPoint2), 1, lineColor);

                // If this is the last point, complete the circle
                if (j == sides - 1)
                {
                    drawingContext.DrawLine(new Line2D(screenPoint2, firstPoint2), 1, lineColor);
                }

                previousPoint2 = screenPoint2;
            }
        }
    }

    public static void DrawCylinder(this DrawingContext2D drawingContext, Camera camera, in CylinderShape shape)
    {
        const int sides = 8;
        var lineColor = new ColorRgbaF(220, 220, 220, 255);

        var firstPoint = Vector2.Zero;
        var previousPoint = Vector2.Zero;
        var firstPointTop = Vector2.Zero;
        var previousPointTop = Vector2.Zero;

        for (var i = 0; i < sides; i++)
        {
            var angle = 2 * MathF.PI * i / sides;
            var point = shape.BottomCenter + new Vector3(MathF.Cos(angle), MathF.Sin(angle), 0) * shape.Radius;
            var screenPoint = camera.WorldToScreenPoint(point).Vector2XY();
            var pointTop = point + new Vector3(0, 0, shape.Height);
            var screenPointTop = camera.WorldToScreenPoint(pointTop).Vector2XY();

            // No line gets drawn on the first iteration
            if (i == 0)
            {
                firstPoint = screenPoint;
                previousPoint = screenPoint;
                firstPointTop = screenPointTop;
                previousPointTop = screenPointTop;
                continue;
            }

            drawingContext.DrawLine(new Line2D(previousPoint, screenPoint), 1, lineColor);
            drawingContext.DrawLine(new Line2D(previousPointTop, screenPointTop), 1, lineColor);
            drawingContext.DrawLine(new Line2D(previousPoint, previousPointTop), 1, lineColor);

            // If this is the last point, complete the cylinder
            if (i == sides - 1)
            {
                drawingContext.DrawLine(new Line2D(screenPoint, firstPoint), 1, lineColor);
                drawingContext.DrawLine(new Line2D(screenPointTop, firstPointTop), 1, lineColor);
                drawingContext.DrawLine(new Line2D(screenPoint, screenPointTop), 1, lineColor);
            }

            previousPoint = screenPoint;
            previousPointTop = screenPointTop;
        }
    }

    public static void DrawBox(this DrawingContext2D drawingContext, Camera camera, in BoxShape shape)
    {
        var strokeColor = new ColorRgbaF(220, 220, 220, 255);

        var rotation = QuaternionUtility.CreateFromYawPitchRoll_ZUp(shape.Angle, 0, 0);

        var xLine = Vector3.Transform(new Vector3(shape.HalfSizeX, 0, 0), rotation);
        var yLine = Vector3.Transform(new Vector3(0, shape.HalfSizeY, 0), rotation);

        DrawBox(drawingContext, camera, strokeColor, shape.BottomCenter, xLine, yLine, shape.Height);
    }

    private static void DrawBox(
        DrawingContext2D drawingContext,
        Camera camera,
        in ColorRgbaF strokeColor,
        in Vector3 worldPos,
        in Vector3 xLine,
        in Vector3 yLine,
        float height)
    {
        var leftSide = xLine;
        var rightSide = -xLine;
        var topSide = yLine;
        var bottomSide = -yLine;

        var ltWorld = worldPos + (leftSide - topSide);
        var rtWorld = worldPos + (rightSide - topSide);
        var rbWorld = worldPos + (rightSide - bottomSide);
        var lbWorld = worldPos + (leftSide - bottomSide);

        var ltScreen = camera.WorldToScreenPoint(ltWorld).Vector2XY();
        var rtScreen = camera.WorldToScreenPoint(rtWorld).Vector2XY();
        var rbScreen = camera.WorldToScreenPoint(rbWorld).Vector2XY();
        var lbScreen = camera.WorldToScreenPoint(lbWorld).Vector2XY();

        drawingContext.DrawLine(new Line2D(ltScreen, lbScreen), 1, strokeColor);
        drawingContext.DrawLine(new Line2D(lbScreen, rbScreen), 1, strokeColor);
        drawingContext.DrawLine(new Line2D(rbScreen, rtScreen), 1, strokeColor);
        drawingContext.DrawLine(new Line2D(rtScreen, ltScreen), 1, strokeColor);

        var heightVector = new Vector3(0, 0, height);
        var ltWorldTop = worldPos + (leftSide - topSide) + heightVector;
        var rtWorldTop = worldPos + (rightSide - topSide) + heightVector;
        var rbWorldTop = worldPos + (rightSide - bottomSide) + heightVector;
        var lbWorldTop = worldPos + (leftSide - bottomSide) + heightVector;

        var ltScreenTop = camera.WorldToScreenPoint(ltWorldTop).Vector2XY();
        var rtScreenTop = camera.WorldToScreenPoint(rtWorldTop).Vector2XY();
        var rbScreenTop = camera.WorldToScreenPoint(rbWorldTop).Vector2XY();
        var lbScreenTop = camera.WorldToScreenPoint(lbWorldTop).Vector2XY();

        drawingContext.DrawLine(new Line2D(ltScreenTop, lbScreenTop), 1, strokeColor);
        drawingContext.DrawLine(new Line2D(lbScreenTop, rbScreenTop), 1, strokeColor);
        drawingContext.DrawLine(new Line2D(rbScreenTop, rtScreenTop), 1, strokeColor);
        drawingContext.DrawLine(new Line2D(rtScreenTop, ltScreenTop), 1, strokeColor);

        drawingContext.DrawLine(new Line2D(ltScreen, ltScreenTop), 1, strokeColor);
        drawingContext.DrawLine(new Line2D(lbScreen, lbScreenTop), 1, strokeColor);
        drawingContext.DrawLine(new Line2D(rbScreen, rbScreenTop), 1, strokeColor);
        drawingContext.DrawLine(new Line2D(rtScreen, rtScreenTop), 1, strokeColor);
    }
}
