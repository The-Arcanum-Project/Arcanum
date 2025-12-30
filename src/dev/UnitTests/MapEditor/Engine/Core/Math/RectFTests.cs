using System.Runtime.CompilerServices;
using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.MapEditor.Engine.Core.Math;

namespace UnitTests.MapEditor.Engine.Core.Math;

[TestFixture]
public class RectFTests
{
   [Test]
   public void Struct_Layout_Is16Bytes()
   {
      Assert.That(Unsafe.SizeOf<RectF>(), Is.EqualTo(16), "RectF should be exactly 16 bytes.");
   }

   #region Properties & Constructor

   [Test]
   public void Constructor_SetsFieldsCorrectly()
   {
      var rect = new RectF(10.5f, 20.5f, 100f, 200f);

      Assert.Multiple(() =>
      {
         Assert.That(rect.X, Is.EqualTo(10.5f));
         Assert.That(rect.Y, Is.EqualTo(20.5f));
         Assert.That(rect.Width, Is.EqualTo(100f));
         Assert.That(rect.Height, Is.EqualTo(200f));
      });
   }

   [Test]
   public void CalculatedProperties_ReturnCorrectBoundaries()
   {
      var rect = new RectF(10f, 20f, 50f, 60f);

      Assert.Multiple(() =>
      {
         Assert.That(rect.Left, Is.EqualTo(10f));
         Assert.That(rect.Top, Is.EqualTo(20f));
         Assert.That(rect.Right, Is.EqualTo(60f)); // 10 + 50
         Assert.That(rect.Bottom, Is.EqualTo(80f)); // 20 + 60
      });
   }

   [Test]
   public void Properties_WorkWithNegativeCoordinates()
   {
      var rect = new RectF(-50f, -50f, 20f, 20f);

      Assert.Multiple(() =>
      {
         Assert.That(rect.Left, Is.EqualTo(-50f));
         Assert.That(rect.Right, Is.EqualTo(-30f));
         Assert.That(rect.Top, Is.EqualTo(-50f));
         Assert.That(rect.Bottom, Is.EqualTo(-30f));
      });
   }

   #endregion

   #region Contains (Vector2I)

   [TestCase(15, 25, ExpectedResult = true)] // Center
   [TestCase(10, 20, ExpectedResult = true)] // Top-Left corner (Inclusive)
   [TestCase(60, 80, ExpectedResult = true)] // Bottom-Right corner (Inclusive due to <= logic)
   [TestCase(9, 25, ExpectedResult = false)] // Too far left
   [TestCase(61, 25, ExpectedResult = false)] // Too far right
   [TestCase(15, 19, ExpectedResult = false)] // Too far up
   [TestCase(15, 81, ExpectedResult = false)] // Too far down
   public bool Contains_BasicScenarios(int px, int py)
   {
      var rect = new RectF(10f, 20f, 50f, 60f); // X:10->60, Y:20->80
      return rect.Contains(new(px, py));
   }

   [Test]
   public void Contains_HandlesNegativeCoordinates()
   {
      // Rect from -100 to -50
      var rect = new RectF(-100f, -100f, 50f, 50f);

      var pointInside = new Vector2I(-75, -75);
      var pointOutside = new Vector2I(-101, -75);

      Assert.Multiple(() =>
      {
         Assert.That(rect.Contains(pointInside), Is.True);
         Assert.That(rect.Contains(pointOutside), Is.False);
      });
   }

   [Test]
   public void Contains_BitwiseLogic_HandlesZeroDeltas()
   {
      // The bitwise logic relies on (point - X) not being negative.
      // If point == X, delta is +0.0f.
      var rect = new RectF(0f, 0f, 10f, 10f);

      Assert.Multiple(() =>
      {
         Assert.That(rect.Contains(new(0, 0)), Is.True);
         Assert.That(rect.Contains(new(10, 10)), Is.True);
      });
   }

   [Test]
   public void Contains_BitwiseLogic_HandlesNegativeDeltaHack()
   {
      // Validating the specific optimization logic:
      // If px < X, (px - X) is negative.
      // (uint)negative_float is a very large number (sign bit set).
      // This large number should be > (uint)Width.

      var rect = new RectF(10f, 10f, 20f, 20f);
      var point = new Vector2I(9, 15); // x is 9, rect.X is 10. dx = -1.0f.

      // Sanity check manually to ensure test environment isn't weird
      var dx = point.X - rect.X;
      var dxInt = BitConverter.SingleToUInt32Bits(dx);
      var wInt = BitConverter.SingleToUInt32Bits(rect.Width);

      Assert.Multiple(() =>
      {
         Assert.That(dx, Is.LessThan(0));
         Assert.That(dxInt, Is.GreaterThan(wInt));
         Assert.That(rect.Contains(point), Is.False);
      });
   }

   #endregion

   #region Intersects

   [Test]
   public void Intersects_StandardOverlap()
   {
      var r1 = new RectF(0, 0, 100, 100);
      var r2 = new RectF(50, 50, 100, 100);

      Assert.Multiple(() =>
      {
         Assert.That(r1.Intersects(r2), Is.True);
         Assert.That(r2.Intersects(r1), Is.True); // Commutative
      });
   }

   [Test]
   public void Intersects_OneInsideAnother()
   {
      var outer = new RectF(0, 0, 100, 100);
      var inner = new RectF(25, 25, 50, 50);

      Assert.Multiple(() =>
      {
         Assert.That(outer.Intersects(inner), Is.True);
         Assert.That(inner.Intersects(outer), Is.True);
      });
   }

   [Test]
   public void Intersects_Disjoint()
   {
      var r1 = new RectF(0, 0, 10, 10);
      var r2 = new RectF(20, 0, 10, 10);

      Assert.That(r1.Intersects(r2), Is.False);
   }

   [Test]
   public void Intersects_TouchingEdges_ReturnsFalse()
   {
      // Your implementation uses strictly less than:
      // (other.X < X + Width) & (X < other.X + other.Width) ...

      var r1 = new RectF(0, 0, 10, 10); // Ends at 10
      var r2 = new RectF(10, 0, 10, 10); // Starts at 10

      // 10 < 0 + 10 (10 < 10) is False.
      Assert.That(r1.Intersects(r2), Is.False, "Touching edges should not count as intersection in this logic.");
   }

   [Test]
   public void Intersects_TouchingCorners_ReturnsFalse()
   {
      var r1 = new RectF(0, 0, 10, 10);
      var r2 = new RectF(10, 10, 10, 10); // Touch at (10,10)

      Assert.That(r1.Intersects(r2), Is.False);
   }

   #endregion

   #region Equality & SIMD

   [Test]
   public void Equals_IdenticalRects_ReturnsTrue()
   {
      var r1 = new RectF(1.1f, 2.2f, 3.3f, 4.4f);
      var r2 = new RectF(1.1f, 2.2f, 3.3f, 4.4f);

      Assert.Multiple(() =>
      {
         Assert.That(r1, Is.EqualTo(r2));
         Assert.That(r1, Is.EqualTo(r2));
         Assert.That(r1, Is.EqualTo(r2));
      });
   }

   [Test]
   public void Equals_DifferentValues_ReturnsFalse()
   {
      var baseRect = new RectF(1, 2, 3, 4);

      var diffX = new RectF(99, 2, 3, 4);
      var diffY = new RectF(1, 99, 3, 4);
      var diffW = new RectF(1, 2, 99, 4);
      var diffH = new RectF(1, 2, 3, 99);

      Assert.Multiple(() =>
      {
         Assert.That(baseRect, Is.Not.EqualTo(diffX));
         Assert.That(baseRect, Is.Not.EqualTo(diffY));
         Assert.That(baseRect, Is.Not.EqualTo(diffW));
         Assert.That(baseRect, Is.Not.EqualTo(diffH));
      });
   }

   [Test]
   public void Equals_ObjectOverload()
   {
      var r1 = new RectF(1, 2, 3, 4);
      object o1 = new RectF(1, 2, 3, 4);
      object o2 = "Not A Rect";
      object? o3 = null;

      Assert.Multiple(() =>
      {
         Assert.That(r1, Is.EqualTo(o1));
         Assert.That(r1, Is.Not.EqualTo(o2));
         Assert.That(r1, Is.Not.EqualTo(o3));
      });
   }

   [Test]
   public void GetHashCode_SameValues_SameHash()
   {
      var r1 = new RectF(10, 20, 30, 40);
      var r2 = new RectF(10, 20, 30, 40);

      Assert.That(r1.GetHashCode(), Is.EqualTo(r2.GetHashCode()));
   }

   [Test]
   public void GetHashCode_DifferentValues_LikelyDifferentHash()
   {
      var r1 = new RectF(10, 20, 30, 40);
      var r2 = new RectF(0, 0, 0, 0);

      Assert.That(r1.GetHashCode(), Is.Not.EqualTo(r2.GetHashCode()));
   }

   #endregion
}