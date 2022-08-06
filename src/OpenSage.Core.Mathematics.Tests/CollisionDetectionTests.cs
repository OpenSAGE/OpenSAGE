using System.Numerics;
using Xunit;

namespace OpenSage.Mathematics.Tests;

public class CollisionDetectionTests
{
    [Theory]
    [InlineData(0, 0, 0, 1, true)]
    [InlineData(1, 0, 0, 1, true)]
    [InlineData(2, 0, 0, 1, true)]
    [InlineData(-2, 0, 0, 1, true)]
    [InlineData(0, -2, 0, 1, true)]
    [InlineData(0, 0, -2, 1, true)]
    [InlineData(2.01f, 0, 0, 1, false)]
    [InlineData(-2.01f, 0, 0, 1, false)]
    [InlineData(0, -2.01f, 0, 1, false)]
    [InlineData(0, 0, -2.01f, 1, false)]
    public void TestSphereSphere(float x, float y, float z, float radius, bool expected)
    {
        var sphere1 = new SphereShape(Vector3.Zero, 1);

        var sphere2 = new SphereShape(new Vector3(x, y, z), radius);

        Assert.Equal(expected, CollisionDetection.IntersectSphereSphere(sphere1, sphere2));
    }

    [Theory]
    [InlineData(0, 0, 0, 1, 0, 0, 0, 1, 1, true)]
    [InlineData(0, 0, 0, 1, 10, 0, 0, 1, 1, false)]
    [InlineData(0, 0, 0, 1, 0, 10, 0, 1, 1, false)]
    [InlineData(0, 0, 0, 1, 0, 0, 10, 1, 1, false)]
    [InlineData(0, 0, 1.5f, 1, 0, 0, 0, 1, 1, true)]
    [InlineData(0, 0, -0.5f, 1, 0, 0, 0, 1, 1, true)]
    [InlineData(0, 0, -1.5f, 1, 0, 0, 0, 1, 1, false)]
    public void TestSphereCylinder(
        float sphereX, float sphereY, float sphereZ, float sphereRadius,
        float cylinderX, float cylinderY, float cylinderZ, float cylinderRadius, float cylinderHeight,
        bool expected)
    {
        var sphere = new SphereShape(new Vector3(sphereX, sphereY, sphereZ), sphereRadius);

        var cylinder = new CylinderShape(new Vector3(cylinderX, cylinderY, cylinderZ), cylinderHeight, cylinderRadius);

        Assert.Equal(expected, CollisionDetection.IntersectSphereCylinder(sphere, cylinder));
    }

    [Theory]
    [InlineData(0, 0, 0, 1, 0, 0, 0, 0, 1, 1, 1, true)]
    [InlineData(0, 0, 0, 1, 5, 0, 0, 0, 1, 1, 1, false)]
    public void TestSphereBox(
        float sphereX, float sphereY, float sphereZ, float sphereRadius,
        float boxX, float boxY, float boxZ, float boxAngle, float boxHalfSizeX, float boxHalfSizeY, float boxHeight,
        bool expected)
    {
        var sphere = new SphereShape(new Vector3(sphereX, sphereY, sphereZ), sphereRadius);

        var box = new BoxShape(new Vector3(boxX, boxY, boxZ), boxAngle, boxHalfSizeX, boxHalfSizeY, boxHeight);

        Assert.Equal(expected, CollisionDetection.IntersectSphereBox(sphere, box));
    }

    [Theory]
    [InlineData(0, 0, 0, 1, 1, true)]
    [InlineData(1, 0, 0, 1, 1, true)]
    [InlineData(2, 0, 0, 1, 1, false)]
    [InlineData(-2, 0, 0, 1, 1, false)]
    [InlineData(0, -2, 0, 1, 1, false)]
    [InlineData(0, 0, -2, 1, 1, true)]
    [InlineData(2.01f, 0, 0, 1, 1, false)]
    [InlineData(-2.01f, 0, 0, 1, 1, false)]
    [InlineData(0, -2.01f, 0, 1, 1, false)]
    [InlineData(0, 0, -2.01f, 1, 1, true)]
    public void TestCylinderCylinder(float x, float y, float z, float height, float radius, bool expected)
    {
        var cylinder1 = new CylinderShape(Vector3.Zero, 1, 1);

        var cylinder2 = new CylinderShape(new Vector3(x, y, z), height, radius);

        Assert.Equal(expected, CollisionDetection.IntersectCylinderCylinder(cylinder1, cylinder2));
    }

    [Theory]
    [InlineData(0, 0, 0, 1, 1, 0, 0, 0, 0, 1, 1, 1, true)]
    [InlineData(0, 0, 0, 1, 1, 5, 0, 0, 0, 1, 1, 1, false)]
    public void TestCylinderBox(
        float cylinderX, float cylinderY, float cylinderZ, float cylinderRadius, float cylinderHeight,
        float boxX, float boxY, float boxZ, float boxAngle, float boxHalfSizeX, float boxHalfSizeY, float boxHeight,
        bool expected)
    {
        var cylinder = new CylinderShape(new Vector3(cylinderX, cylinderY, cylinderZ), cylinderHeight, cylinderRadius);

        var box = new BoxShape(new Vector3(boxX, boxY, boxZ), boxAngle, boxHalfSizeX, boxHalfSizeY, boxHeight);

        Assert.Equal(expected, CollisionDetection.IntersectCylinderBox(cylinder, box));
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, true)]
    [InlineData(0, 0, 0, 0, 1, 1, 1, 1.9f, 0, 0, 0, 1, 1, 1, true)]
    [InlineData(0, 0, 0, 0, 1, 1, 1, 2.0f, 0, 0, 0, 1, 1, 1, true)]
    [InlineData(0, 0, 0, 0, 1, 1, 1, 2.1f, 0, 0, 0, 1, 1, 1, false)]
    [InlineData(0, 0, 0, 0, 1, 1, 1, 5, 0, 0, 0, 1, 1, 1, false)]
    [InlineData(0, 0, 0, 0, 5, 1, 1, 0, 3, 0, 0, 1, 1, 1, false)]
    [InlineData(0, 0, 0, MathUtility.PiOver2, 5, 1, 1, 0, 3, 0, 0, 1, 1, 1, true)]
    public void TestBoxBox(
        float box1X, float box1Y, float box1Z, float box1Angle, float box1HalfSizeX, float box1HalfSizeY, float box1Height,
        float box2X, float box2Y, float box2Z, float box2Angle, float box2HalfSizeX, float box2HalfSizeY, float box2Height,
        bool expected)
    {
        var box1 = new BoxShape(new Vector3(box1X, box1Y, box1Z), box1Angle, box1HalfSizeX, box1HalfSizeY, box1Height);

        var box2 = new BoxShape(new Vector3(box2X, box2Y, box2Z), box2Angle, box2HalfSizeX, box2HalfSizeY, box2Height);

        Assert.Equal(expected, CollisionDetection.IntersectBoxBox(box1, box2));
    }
}
