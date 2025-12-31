using System.Numerics;
using Arcanum.Core.MapEditor.Engine.Core.Math;
using Arcanum.Core.MapEditor.Engine.Core.Spatial;

namespace UnitTests.MapEditor.Engine.Core.Spatial;

[TestFixture]
public class QuadTreeTests
{
   private class TestEntity : I3DEntity
   {
      public int Id { get; } = Guid.NewGuid().GetHashCode();

      public Vector3 Position3D { get; set; }
      public Quaternion Rotation3D { get; set; }
      public Vector3 Scale3D { get; set; } = Vector3.One;
      public Vector3 LocalSize3D { get; set; }
      public BoundingBoxF Bounds3D { get; private set; }

      // Constructor mimics 2D inputs but maps them to 3D (X, Z) plane
      public TestEntity(float x, float z, float w, float h)
      {
         SetBounds(x, z, w, h);
      }

      public void SetBounds(float x, float z, float w, float h)
      {
         var min = new Vector3(x, 0, z);
         var max = new Vector3(x + w, 10, z + h); // Arbitrary height
         Bounds3D = new(min, max);
         Position3D = Bounds3D.Center;
      }
   }

   private QuadTree<TestEntity> _quadTree;
   private RectF _worldBounds;

   [SetUp]
   public void Setup()
   {
      // World is 0,0 to 100,100 on the X/Z plane
      _worldBounds = new(0, 0, 100, 100);

      // Pass by ref as required by new constructor
      _quadTree = new(ref _worldBounds, maxObjectsPerNode: 4, maxDepth: 5);
   }

   [Test]
   public void Insert_SingleItem_ShouldBeFoundInQuery()
   {
      // 10, 10 on the XZ plane
      var entity = new TestEntity(10, 10, 5, 5);
      _quadTree.Insert(entity);

      // Query a rect that fully encompasses the entity
      var range = new RectF(0, 0, 20, 20);
      var results = _quadTree.Query(range);

      Assert.That(results, Has.Count.EqualTo(1));
      Assert.That(results[0].Id, Is.EqualTo(entity.Id));
   }

   [Test]
   public void Insert_ItemOutsideQueryRange_ShouldReturnEmpty()
   {
      var entity = new TestEntity(10, 10, 5, 5);
      _quadTree.Insert(entity);

      // Query a rect far away (80,80)
      var results = _quadTree.Query(new(80, 80, 10, 10));

      Assert.That(results, Is.Empty);
   }

   [Test]
   public void Splitting_WhenThresholdExceeded_ShouldDistributeItems()
   {
      // Tree splits after 4 items. Insert 5 into Top-Left (0-50, 0-50).
      var e1 = new TestEntity(1, 1, 2, 2);
      var e2 = new TestEntity(2, 2, 2, 2);
      var e3 = new TestEntity(3, 3, 2, 2);
      var e4 = new TestEntity(4, 4, 2, 2);
      var e5 = new TestEntity(5, 5, 2, 2); // Forces split

      _quadTree.Insert(e1);
      _quadTree.Insert(e2);
      _quadTree.Insert(e3);
      _quadTree.Insert(e4);
      _quadTree.Insert(e5);

      // Verify all are retrievable via root query
      var all = _quadTree.Query(_worldBounds);
      Assert.That(all, Has.Count.EqualTo(5));

      // Verify specific query in that quadrant works
      var quadrantQuery = _quadTree.Query(new(0, 0, 10, 10));
      Assert.That(quadrantQuery, Has.Count.EqualTo(5));
   }

   [Test]
   public void StraddlingItems_ShouldStayInParentNode()
   {
      // World center is 50, 50.
      // Item at X=48, Width=4 (Ends at 52). Crosses X=50.
      var straddler = new TestEntity(48, 10, 4, 4);

      _quadTree.Insert(straddler);
      // Add filler to force split
      _quadTree.Insert(new(10, 10, 2, 2));
      _quadTree.Insert(new(12, 12, 2, 2));
      _quadTree.Insert(new(14, 14, 2, 2));
      _quadTree.Insert(new(16, 16, 2, 2));

      // Query Left side (0 to 49)
      var leftResults = _quadTree.Query(new(0, 0, 49, 100));
      Assert.That(leftResults.Any(x => x.Id == straddler.Id), Is.True, "Should be found by Left query");

      // Query Right side (51 to 100)
      var rightResults = _quadTree.Query(new(51, 0, 49, 100));
      Assert.That(rightResults.Any(x => x.Id == straddler.Id), Is.True, "Should be found by Right query");
   }

   [Test]
   public void Remove_ExistingItem_ShouldReturnTrueAndRemove()
   {
      var entity = new TestEntity(20, 20, 10, 10);
      _quadTree.Insert(entity);

      var removed = _quadTree.Remove(entity);
      var results = _quadTree.Query(_worldBounds);

      Assert.Multiple(() =>
      {
         Assert.That(removed, Is.True);
         Assert.That(results, Is.Empty);
      });
   }

   [Test]
   public void Remove_NonExistingItem_ShouldReturnFalse()
   {
      var entity = new TestEntity(20, 20, 10, 10);
      var removed = _quadTree.Remove(entity);
      Assert.That(removed, Is.False);
   }

   [Test]
   public void Move_ShouldRelocateEntity()
   {
      // Start at 10, 10
      var entity = new TestEntity(10, 10, 5, 5);
      _quadTree.Insert(entity);

      Assert.Multiple(() =>
      {
         // Verify initial position
         Assert.That(_quadTree.Query(new(5, 5, 15, 15)), Has.Count.EqualTo(1));
         Assert.That(_quadTree.Query(new(80, 80, 10, 10)), Is.Empty);
      });

      // Move to 85, 85 (Z is Y in the constructor of our test entity)
      const float newX = 85f;
      const float newZ = 85f;

      entity.SetBounds(newX, newZ, 5, 5);
      var newPos3D = new Vector3(newX, 0, newZ);

      // Action
      _quadTree.Move(entity, newPos3D);

      Assert.Multiple(() =>
      {
         // Should be gone from old spot
         Assert.That(_quadTree.Query(new(5, 5, 15, 15)), Is.Empty);
         // Should be found in new spot
         Assert.That(_quadTree.Query(new(80, 80, 10, 10)), Has.Count.EqualTo(1));
      });
   }

   [Test]
   public void QueryPoint_ShouldDetectItemUnderCursor()
   {
      var entity = new TestEntity(50, 50, 10, 10); // X=50, Z=50
      _quadTree.Insert(entity);

      // Click at 55, 55 (Center of box)
      var hits = _quadTree.QueryPoint(new(55, 55), radius: 1.0f);

      Assert.That(hits, Has.Count.EqualTo(1));
   }

   [Test]
   public void StressTest_ManyItems()
   {
      const int count = 1000;
      var rng = new Random(42);

      for (var i = 0; i < count; i++)
      {
         float x = rng.Next(0, 90);
         float z = rng.Next(0, 90);
         // Ensure we create a valid 3D entity mapping Z correctly
         var ent = new TestEntity(x, z, 2, 2);
         _quadTree.Insert(ent);
      }

      var results = _quadTree.Query(_worldBounds);
      var uniqueIds = results.Select(e => e.Id).Distinct().Count();

      Assert.That(uniqueIds, Is.EqualTo(count));
   }
}