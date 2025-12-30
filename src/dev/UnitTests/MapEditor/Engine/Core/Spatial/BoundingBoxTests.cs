using System.Numerics;

namespace UnitTests.MapEditor.Engine.Core.Spatial;

[TestFixture]
public class BoundingBoxFTests
{
   private static BoundingBoxF CreateStandardBox() => new(Vector3.Zero, new(10, 20, 30));

   [Test]
   public void Constructor_SetsMinAndMaxCorrectly()
   {
      var min = new Vector3(-5, -5, -5);
      var max = new Vector3(5, 5, 5);
      var box = new BoundingBoxF(min, max);

      Assert.Multiple(() =>
      {
         Assert.That(box.Min, Is.EqualTo(min));
         Assert.That(box.Max, Is.EqualTo(max));
      });
   }

   [Test]
   public void ComputedProperties_CalculateCorrectDimensions()
   {
      // Min(0,0,0) -> Max(10,20,30)
      var box = CreateStandardBox();

      Assert.Multiple(() =>
      {
         Assert.That(box.Width, Is.EqualTo(10f));
         Assert.That(box.Height, Is.EqualTo(20f));
         Assert.That(box.Depth, Is.EqualTo(30f));

         Assert.That(box.Size, Is.EqualTo(new Vector3(10, 20, 30)));
      });
   }

   [Test]
   public void Center_CalculatesCorrectMidpoint()
   {
      // Min(0,0,0) -> Max(10,20,30). Center should be (5, 10, 15)
      var box = CreateStandardBox();
      var expectedCenter = new Vector3(5, 10, 15);

      Assert.That(box.Center, Is.EqualTo(expectedCenter));
   }

   [Test]
   public void Center_WithNegativeCoordinates_CalculatesCorrectly()
   {
      // Min(-10, -10, -10) -> Max(10, 10, 10). Center should be (0,0,0)
      var box = new BoundingBoxF(new(-10), new(10));
      Assert.That(box.Center, Is.EqualTo(Vector3.Zero));
   }

   [Test]
   public void CreateFromCenterSize_GeneratesCorrectBounds()
   {
      var center = new Vector3(10, 10, 10);
      var size = new Vector3(4, 6, 8);

      var box = BoundingBoxF.CreateFromCenterSize(center, size);

      Assert.Multiple(() =>
      {
         // Half size = (2, 3, 4)
         // Min = Center - Half = (8, 7, 6)
         // Max = Center + Half = (12, 13, 14)

         Assert.That(box.Min, Is.EqualTo(new Vector3(8, 7, 6)));
         Assert.That(box.Max, Is.EqualTo(new Vector3(12, 13, 14)));
         Assert.That(box.Width, Is.EqualTo(4));
         Assert.That(box.Height, Is.EqualTo(6));
      });
   }

   [Test]
   public void CreateFromPoints_StandardList_FindsMinMax()
   {
      var points = new[] { new Vector3(1, 5, 1), new Vector3(-5, 0, 0), new Vector3(10, 10, 10), new Vector3(0, -10, 0) };

      var box = BoundingBoxF.CreateFromPoints(points);

      Assert.Multiple(() =>
      {
         // Min X: -5, Min Y: -10, Min Z: 0 (from 3rd param of -5,0,0 or 0,-10,0? No, 0,-10,0 is 0. -5,0,0 is 0. 1,5,1 is 1.)
         // Let's trace manually:
         // Xs: 1, -5, 10, 0 -> Min -5, Max 10
         // Ys: 5, 0, 10, -10 -> Min -10, Max 10
         // Zs: 1, 0, 10, 0 -> Min 0, Max 10

         Assert.That(box.Min, Is.EqualTo(new Vector3(-5, -10, 0)));
         Assert.That(box.Max, Is.EqualTo(new Vector3(10, 10, 10)));
      });
   }

   [Test]
   public void CreateFromPoints_SinglePoint_MinEqualsMax()
   {
      var points = new[] { new Vector3(5, 5, 5) };
      var box = BoundingBoxF.CreateFromPoints(points);

      Assert.Multiple(() =>
      {
         Assert.That(box.Min, Is.EqualTo(new Vector3(5, 5, 5)));
         Assert.That(box.Max, Is.EqualTo(new Vector3(5, 5, 5)));
         Assert.That(box.Size, Is.EqualTo(Vector3.Zero));
      });
   }

   [Test]
   public void CreateFromPoints_EmptySpan_ReturnsDefault()
   {
      var box = BoundingBoxF.CreateFromPoints(ReadOnlySpan<Vector3>.Empty);

      // Assuming default struct is all zeros
      Assert.That(box, Is.EqualTo(default(BoundingBoxF)));
      Assert.That(box.Min, Is.EqualTo(Vector3.Zero));
   }

   #region Contains Tests (Validating SIMD/Branchless Logic)

   [Test]
   public void Contains_PointInside_ReturnsTrue()
   {
      var box = new BoundingBoxF(Vector3.Zero, new(10, 10, 10));
      var point = new Vector3(5, 5, 5);

      Assert.That(box.Contains(point), Is.True);
   }

   [Test]
   public void Contains_PointOnBoundary_ReturnsTrue()
   {
      // The code uses <= and >= so it should be inclusive
      var box = new BoundingBoxF(Vector3.Zero, new(10, 10, 10));

      Assert.Multiple(() =>
      {
         Assert.That(box.Contains(Vector3.Zero), Is.True, "Should contain Min boundary");
         Assert.That(box.Contains(new(10, 10, 10)), Is.True, "Should contain Max boundary");
         Assert.That(box.Contains(new(5, 10, 5)), Is.True, "Should contain Face boundary");
      });
   }

   [Test]
   public void Contains_PointOutside_ReturnsFalse()
   {
      var box = new BoundingBoxF(Vector3.Zero, new(10, 10, 10));

      Assert.Multiple(() =>
      {
         // Test X axis failure
         Assert.That(box.Contains(new(11, 5, 5)), Is.False);
         Assert.That(box.Contains(new(-1, 5, 5)), Is.False);

         // Test Y axis failure
         Assert.That(box.Contains(new(5, 11, 5)), Is.False);

         // Test Z axis failure
         Assert.That(box.Contains(new(5, 5, 11)), Is.False);
      });
   }

   #endregion

   #region Intersects Tests (Validating SIMD/Branchless Logic)

   [Test]
   public void Intersects_OverlappingBoxes_ReturnsTrue()
   {
      // Box A: 0,0,0 -> 10,10,10
      var boxA = new BoundingBoxF(Vector3.Zero, new(10));
      // Box B: 5,5,5 -> 15,15,15 (Starts inside A, ends outside)
      var boxB = new BoundingBoxF(new(5), new(15));

      Assert.Multiple(() =>
      {
         Assert.That(boxA.Intersects(boxB), Is.True);
         Assert.That(boxB.Intersects(boxA), Is.True);
      });
   }

   [Test]
   public void Intersects_BoxInsideBox_ReturnsTrue()
   {
      var outer = new BoundingBoxF(Vector3.Zero, new(20));
      var inner = new BoundingBoxF(new(5), new(15));

      Assert.Multiple(() =>
      {
         Assert.That(outer.Intersects(inner), Is.True);
         Assert.That(inner.Intersects(outer), Is.True);
      });
   }

   [Test]
   public void Intersects_TouchingBoxes_ReturnsTrue()
   {
      // Inclusive boundaries usually mean touching returns true
      // Box A: 0 -> 10
      var boxA = new BoundingBoxF(Vector3.Zero, new(10));
      // Box B: 10 -> 20 (Touches at 10)
      var boxB = new BoundingBoxF(new(10, 0, 0), new(20, 10, 10));

      Assert.That(boxA.Intersects(boxB), Is.True);
   }

   [Test]
   public void Intersects_DisjointBoxes_ReturnsFalse()
   {
      var boxA = new BoundingBoxF(Vector3.Zero, new(10));

      // Separated by X
      var boxX = new BoundingBoxF(new(11, 0, 0), new(20, 10, 10));
      Assert.That(boxA.Intersects(boxX), Is.False);

      // Separated by Y
      var boxY = new BoundingBoxF(new(0, 11, 0), new(10, 20, 10));
      Assert.That(boxA.Intersects(boxY), Is.False);
   }

   #endregion

   #region Equality Tests

   [Test]
   public void Equals_IdenticalBoxes_ReturnsTrue()
   {
      var b1 = new BoundingBoxF(Vector3.Zero, Vector3.One);
      var b2 = new BoundingBoxF(Vector3.Zero, Vector3.One);

      Assert.Multiple(() =>
      {
         Assert.That(b1.Equals(b2), Is.True);
         Assert.That(b1 == b2, Is.True);
         Assert.That(b1 != b2, Is.False);
      });
   }

   [Test]
   public void Equals_DifferentBoxes_ReturnsFalse()
   {
      var b1 = new BoundingBoxF(Vector3.Zero, Vector3.One);
      var b2 = new BoundingBoxF(Vector3.Zero, new(1, 1, 2)); // Slightly different Z

      Assert.That(b1, Is.Not.EqualTo(b2));
      Assert.That(b1, Is.Not.EqualTo(b2));
      Assert.That(b1, Is.Not.EqualTo(b2));
   }

   [Test]
   public void GetHashCode_SameValues_ReturnsSameHash()
   {
      var b1 = new BoundingBoxF(new(1, 2, 3), new(4, 5, 6));
      var b2 = new BoundingBoxF(new(1, 2, 3), new(4, 5, 6));

      Assert.That(b1.GetHashCode(), Is.EqualTo(b2.GetHashCode()));
   }

   #endregion
}