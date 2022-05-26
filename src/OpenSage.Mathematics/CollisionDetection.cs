using System;
using System.Numerics;

namespace OpenSage.Mathematics;

public static class CollisionDetection
{
    public static bool IntersectSphereSphere(in SphereShape sphere1, in SphereShape sphere2)
    {
        if (!IntersectZ(sphere1, sphere2))
        {
            return false;
        }

        var distanceSquared = Vector3.DistanceSquared(sphere1.Center, sphere2.Center);

        var maxDistance = sphere1.Radius + sphere2.Radius;

        return distanceSquared <= (maxDistance * maxDistance);
    }

    public static bool IntersectSphereCylinder(in SphereShape sphere, in CylinderShape cylinder)
    {
        if (!IntersectZ(sphere, cylinder))
        {
            return false;
        }

        var sphereZ = sphere.Center.Z;

        // Is the sphere center somewhere between the bottom and the top of the cylinder?
        // If so, we can do a circle-circle test as though the 3D objects were projected into 2D
        // on the X-Y plane.
        if (cylinder.BottomCenter.Z <= sphereZ && sphereZ <= cylinder.MaxZ)
        {
            return IntersectCircleCircle(
                sphere.GetProjectedCircle(),
                cylinder.GetProjectedCircle());
        }

        // Otherwise, we need to "shrink" the sphere to account for it being below the bottom,
        // or above the top, of the cylinder. We can use Pythagoras to find the "base" of the
        // triangle formed by:
        // - base is X distance from sphere center to edge of new sphere; this is what we want to calculate
        // - hypotenuse is sphere radius
        // - height is Z distance between cylinder top (or bottom) and sphere center

        var deltaZ = (cylinder.BottomCenter.Z <= sphereZ)
            ? sphereZ - cylinder.MaxZ
            : cylinder.BottomCenter.Z - sphereZ;

        var projectedRadius = MathF.Sqrt(sphere.Radius * sphere.Radius - deltaZ * deltaZ);

        // Test intersection of sphere and cylinder projected down to 2D circles.
        return IntersectCircleCircle(
            sphere.GetProjectedCircle(projectedRadius),
            cylinder.GetProjectedCircle());
    }

    public static bool IntersectSphereBox(in SphereShape sphere, in BoxShape box)
    {
        if (!IntersectZ(sphere, box))
        {
            return false;
        }

        var sphereZ = sphere.Center.Z;

        // Is the sphere center somewhere between the bottom and the top of the box?
        // If so, we can do a circle-circle test as though the 3D objects were projected into 2D
        // on the X-Y plane.
        if (box.BottomCenter.Z <= sphereZ && sphereZ <= box.MaxZ)
        {
            return IntersectCircleRotatedRectangle(
                sphere.GetProjectedCircle(),
                box.GetProjectedRectangle());
        }

        // Otherwise, we need to "shrink" the sphere to account for it being below the bottom,
        // or above the top, of the box. We can use Pythagoras to find the "base" of the
        // triangle formed by:
        // - base is X distance from sphere center to edge of new sphere; this is what we want to calculate
        // - hypotenuse is sphere radius
        // - height is Z distance between box top (or bottom) and sphere center

        var deltaZ = (box.BottomCenter.Z <= sphereZ)
            ? sphereZ - box.MaxZ
            : box.BottomCenter.Z - sphereZ;

        var projectedRadius = MathF.Sqrt(sphere.Radius * sphere.Radius - deltaZ * deltaZ);

        // Test intersection of sphere and box projected down to 2D shapes.
        return IntersectCircleRotatedRectangle(
            sphere.GetProjectedCircle(projectedRadius),
            box.GetProjectedRectangle());
    }

    public static bool IntersectCylinderCylinder(in CylinderShape cylinder1, in CylinderShape cylinder2)
    {
        if (!IntersectZ(cylinder1, cylinder2))
        {
            return false;
        }

        // Test intersection of cylinders projected down to 2D shapes.
        return IntersectCircleCircle(
            cylinder1.GetProjectedCircle(),
            cylinder2.GetProjectedCircle());
    }

    public static bool IntersectCylinderBox(in CylinderShape cylinder, in BoxShape box)
    {
        if (!IntersectZ(cylinder, box))
        {
            return false;
        }

        // Test intersection of cylinder and box projected down to 2D shapes.
        return IntersectCircleRotatedRectangle(
            cylinder.GetProjectedCircle(),
            box.GetProjectedRectangle());
    }

    public static bool IntersectBoxBox(in BoxShape box1, in BoxShape box2)
    {
        if (!IntersectZ(box1, box2))
        {
            return false;
        }

        // Test intersection of boxes projected down to 2D shapes.
        return IntersectRotatedRectangleRotatedRectangle(
            box1.GetProjectedRectangle(),
            box2.GetProjectedRectangle());
    }

    private static bool IntersectCircleCircle(in CircleShape circle1, in CircleShape circle2)
    {
        var deltaX = circle2.Center.X - circle1.Center.X;
        var deltaY = circle2.Center.Y - circle1.Center.Y;

        var maxDistance = circle1.Radius + circle2.Radius;

        return deltaX * deltaX + deltaY * deltaY <= maxDistance;
    }

    private static bool IntersectCircleRotatedRectangle(in CircleShape circle, in RotatedRectangleShape rectangle)
    {
        // This algorithm is from http://www.migapro.com/circle-and-rotated-rectangle-collision-detection/
        // The basic idea is to rotate the circle around the rectangle's center point,
        // so that the rectangle becomes axis-aligned.
        // Then we're dealing with an AABB-circle intersection.

        // Rotate circle's center point into rectangle's space.
        Vector2 circleCenterInRectangleSpace;
        circleCenterInRectangleSpace.X =
            MathF.Cos(rectangle.Angle) * (circle.Center.X - rectangle.Center.X) -
            MathF.Sin(rectangle.Angle) * (circle.Center.Y - rectangle.Center.Y) +
            rectangle.Center.X;
        circleCenterInRectangleSpace.Y =
            MathF.Sin(rectangle.Angle) * (circle.Center.X - rectangle.Center.X) +
            MathF.Cos(rectangle.Angle) * (circle.Center.Y - rectangle.Center.Y) +
            rectangle.Center.Y;

        return IntersectCircleRectangle(
            new CircleShape(circleCenterInRectangleSpace, circle.Radius),
            new RectangleShape(rectangle.Center, rectangle.HalfSizeX, rectangle.HalfSizeY));
    }

    private static bool IntersectCircleRectangle(in CircleShape circle, in RectangleShape rectangle)
    {
        // Closest point in the rectangle to the center of the circle.
        Vector2 closestPointOnRectangle;

        // Find the closest X point from center of circle.
        if (circle.Center.X < rectangle.MinX)
        {
            closestPointOnRectangle.X = rectangle.MinX;
        }
        else if (circle.Center.X > rectangle.MaxX)
        {
            closestPointOnRectangle.X = rectangle.MaxX;
        }
        else
        {
            closestPointOnRectangle.X = circle.Center.X;
        }

        // Find the closest Y point from center of circle.
        if (circle.Center.Y < rectangle.MinY)
        {
            closestPointOnRectangle.Y = rectangle.MinY;
        }
        else if (circle.Center.Y > rectangle.MaxY)
        {
            closestPointOnRectangle.Y = rectangle.MaxY;
        }
        else
        {
            closestPointOnRectangle.Y = circle.Center.Y;
        }

        // Determine collision
        var distance = Vector2.Distance(circle.Center, closestPointOnRectangle);
        return distance < circle.Radius;
    }

    private static bool IntersectRotatedRectangleRotatedRectangle(in RotatedRectangleShape rectangle1, in RotatedRectangleShape rectangle2)
    {
        // Based on
        // https://www.gamedev.net/articles/programming/general-and-gameplay-programming/2d-rotated-rectangle-collision-r2604
        // https://www.habrador.com/tutorials/math/7-rectangle-rectangle-intersection/

        var axis1 = rectangle1.TopRight - rectangle1.TopLeft;
        var axis2 = rectangle1.TopRight - rectangle1.BottomRight;

        var axis3 = rectangle2.TopLeft - rectangle2.BottomLeft;
        var axis4 = rectangle2.TopLeft - rectangle2.TopRight;

        static (float Min, float Max) ComputeMinMax(Vector2 axis, in RotatedRectangleShape rect)
        {
            var proj1 = Vector2.Dot(axis, rect.TopLeft);
            var proj2 = Vector2.Dot(axis, rect.TopRight);
            var proj3 = Vector2.Dot(axis, rect.BottomLeft);
            var proj4 = Vector2.Dot(axis, rect.BottomRight);

            var min = Math.Min(proj1, Math.Min(proj2, Math.Min(proj3, proj4)));
            var max = Math.Max(proj1, Math.Max(proj2, Math.Max(proj3, proj4)));

            return (min, max);
        }

        static bool ProjectionsIntersect(Vector2 axis, in RotatedRectangleShape a, in RotatedRectangleShape b)
        {
            var (min1, max1) = ComputeMinMax(axis, a);
            var (min2, max2) = ComputeMinMax(axis, b);
            return min1 <= max2 && min2 <= max1;
        }

        if (!ProjectionsIntersect(axis1, rectangle1, rectangle2))
        {
            return false;
        }

        if (!ProjectionsIntersect(axis2, rectangle1, rectangle2))
        {
            return false;
        }

        if (!ProjectionsIntersect(axis3, rectangle1, rectangle2))
        {
            return false;
        }

        if (!ProjectionsIntersect(axis4, rectangle1, rectangle2))
        {
            return false;
        }

        return true;
    }

    private static bool IntersectZ<T, U>(T shape1, U shape2)
        where T : ICollisionShape3D
        where U : ICollisionShape3D
    {
        return shape1.MaxZ >= shape2.MinZ || shape2.MaxZ >= shape1.MinZ;
    }
}

public interface ICollisionShape3D
{
    float MinZ { get; }
    float MaxZ { get; }
}

public readonly record struct SphereShape(in Vector3 Center, float Radius)
    : ICollisionShape3D
{
    public float MinZ => Center.Z - Radius;
    public float MaxZ => Center.Z + Radius;

    public CircleShape GetProjectedCircle() => new CircleShape(Center.Vector2XY(), Radius);

    public CircleShape GetProjectedCircle(float radius) => new CircleShape(Center.Vector2XY(), radius);
}

public readonly record struct CylinderShape(in Vector3 BottomCenter, float Height, float Radius)
    : ICollisionShape3D
{
    public float MinZ => BottomCenter.Z;
    public float MaxZ => BottomCenter.Z + Height;

    public CircleShape GetProjectedCircle() => new CircleShape(BottomCenter.Vector2XY(), Radius);
}

public readonly record struct BoxShape(in Vector3 BottomCenter, float Angle, float HalfSizeX, float HalfSizeY, float Height)
    : ICollisionShape3D
{
    public float MinZ => BottomCenter.Z;
    public float MaxZ => BottomCenter.Z + Height;

    public RotatedRectangleShape GetProjectedRectangle() => new RotatedRectangleShape(BottomCenter.Vector2XY(), Angle, HalfSizeX, HalfSizeY);
}

public readonly record struct CircleShape(in Vector2 Center, float Radius);

public readonly record struct RotatedRectangleShape(in Vector2 Center, float Angle, float HalfSizeX, float HalfSizeY)
{
    public Vector2 TopLeft => new Vector2(Center.X - HalfSizeX, Center.Y - HalfSizeY).RotateAroundPoint(Center, Angle);
    public Vector2 TopRight => new Vector2(Center.X + HalfSizeX, Center.Y - HalfSizeY).RotateAroundPoint(Center, Angle);
    public Vector2 BottomLeft => new Vector2(Center.X - HalfSizeX, Center.Y + HalfSizeY).RotateAroundPoint(Center, Angle);
    public Vector2 BottomRight => new Vector2(Center.X + HalfSizeX, Center.Y + HalfSizeY).RotateAroundPoint(Center, Angle);
}

public readonly record struct RectangleShape(in Vector2 Center, float HalfSizeX, float HalfSizeY)
{
    public float MinX => Center.X - HalfSizeX;
    public float MaxX => Center.X + HalfSizeX;
    public float MinY => Center.Y - HalfSizeY;
    public float MaxY => Center.Y + HalfSizeY;
}
