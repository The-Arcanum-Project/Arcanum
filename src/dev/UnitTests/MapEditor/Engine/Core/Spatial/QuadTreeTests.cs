using Arcanum.Core.CoreSystems.Parsing.MapParsing.Geometry;
using Arcanum.Core.MapEditor.Engine.Core.Math;
using Arcanum.Core.MapEditor.Engine.Core.Spatial;

namespace UnitTests.MapEditor.Engine.Core.Spatial;

[TestFixture]
public class QuadTreeTests
{
   // A concrete implementation of ISpatialEntity for testing purposes
   private class TestEntity(float x, float y, float w, float h)
      : ISpatialEntity
   {
      public int Id { get; } = Guid.NewGuid().GetHashCode();
      public Vector2I Position2D { get; set; } = new(0, 0);
      public RectF Bounds { get; set; } = new(x, y, w, h);
   }

   private QuadTree<TestEntity> _quadTree;
   private RectF _worldBounds;

   [SetUp]
   public void Setup()
   {
      // Create a world from (0,0) to (100,100)
      _worldBounds = new(0, 0, 100, 100);

      // Max 4 objects per node, Max depth 5
      _quadTree = new(ref _worldBounds, maxObjectsPerNode: 4, maxDepth: 5);
   }

   [Test]
   public void Insert_SingleItem_ShouldBeFoundInQuery()
   {
      var entity = new TestEntity(10, 10, 5, 5);
      _quadTree.Insert(entity);

      // Query a rect that fully encompasses the entity
      var results = _quadTree.Query(new(0, 0, 20, 20));

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
      // Tree is configured to split after 4 items.
      // We will insert 5 items into the Top-Left quadrant (0-50, 0-50).

      var e1 = new TestEntity(1, 1, 2, 2);
      var e2 = new TestEntity(2, 2, 2, 2);
      var e3 = new TestEntity(3, 3, 2, 2);
      var e4 = new TestEntity(4, 4, 2, 2);
      var e5 = new TestEntity(5, 5, 2, 2); // This forces the split

      _quadTree.Insert(e1);
      _quadTree.Insert(e2);
      _quadTree.Insert(e3);
      _quadTree.Insert(e4);
      _quadTree.Insert(e5);

      // Verify all are still retrievable via a root query
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
      // Create an item that crosses the vertical center line (X=50).
      // It sits at X=48 with Width=4 (ends at 52).
      var straddler = new TestEntity(48, 10, 4, 4);

      // Add enough items to force a split if possible, though the straddler 
      // shouldn't go down to children.
      _quadTree.Insert(straddler);
      _quadTree.Insert(new(10, 10, 2, 2));
      _quadTree.Insert(new(12, 12, 2, 2));
      _quadTree.Insert(new(14, 14, 2, 2));
      _quadTree.Insert(new(16, 16, 2, 2)); // Trigger split

      // Query specifically the left side
      var leftResults = _quadTree.Query(new(0, 0, 49, 100));
      Assert.That(leftResults.Any(x => x.Id == straddler.Id), Is.True, "Left query should find it");

      // Query specifically the right side
      var rightResults = _quadTree.Query(new(51, 0, 49, 100));
      Assert.That(rightResults.Any(x => x.Id == straddler.Id), Is.True, "Right query should find it");
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
      // Not inserting it

      var removed = _quadTree.Remove(entity);

      Assert.That(removed, Is.False);
   }

   [Test]
   public void Move_ShouldRelocateEntity()
   {
      var entity = new TestEntity(10, 10, 5, 5);
      _quadTree.Insert(entity);

      Assert.Multiple(() =>
      {
         Assert.That(_quadTree.Query(new(5, 5, 15, 15)), Has.Count.EqualTo(1));
         Assert.That(_quadTree.Query(new(80, 80, 10, 10)), Is.Empty);
      });

      var newPos = new Vector2I(85, 85);
      entity.Bounds = new(newPos.X, newPos.Y, 5, 5);

      _quadTree.Move(entity, newPos);

      Assert.Multiple(() =>
      {
         Assert.That(_quadTree.Query(new(5, 5, 15, 15)), Is.Empty);
         Assert.That(_quadTree.Query(new(80, 80, 10, 10)), Has.Count.EqualTo(1));
      });
   }

   [Test]
   public void Move_IntoDifferentQuadrant_ShouldWorkAfterSplit()
   {
      // Fill Top-Left to force split
      for (var i = 0; i < 5; i++)
         _quadTree.Insert(new(5, 5, 2, 2));

      var mover = new TestEntity(10, 10, 2, 2);
      _quadTree.Insert(mover);

      // Move 'mover' to Bottom-Right (80, 80)
      var newPos = new Vector2I(80, 80);
      _quadTree.Move(mover, newPos);

      var result = _quadTree.Query(new(70, 70, 20, 20));
      Assert.That(result, Has.Count.EqualTo(1));
      Assert.That(result[0].Id, Is.EqualTo(mover.Id));
   }

   [Test]
   public void QueryPoint_ShouldDetectItemUnderCursor()
   {
      var entity = new TestEntity(50, 50, 10, 10); // Center at 55, 55 roughly
      _quadTree.Insert(entity);

      // Click exactly inside the box
      // Box is X:50->60, Y:50->60
      var hits = _quadTree.QueryPoint(new(55, 55), radius: 1.0f);

      Assert.That(hits, Has.Count.EqualTo(1));
   }

   [Test]
   public void QueryPoint_WithRadius_ShouldDetectNearbyItems()
   {
      var entity = new TestEntity(50, 50, 10, 10);
      _quadTree.Insert(entity);

      // Click at 48, 48. This is OUTSIDE the box (starts at 50,50).
      // But if radius is 5, it should overlap.
      var hits = _quadTree.QueryPoint(new(48, 48), radius: 5.0f);

      Assert.That(hits, Has.Count.EqualTo(1));
   }

   [Test]
   public void Clear_ShouldRemoveAllItems()
   {
      _quadTree.Insert(new(10, 10, 5, 5));
      _quadTree.Insert(new(60, 60, 5, 5));

      _quadTree.Clear();

      var results = _quadTree.Query(_worldBounds);
      Assert.That(results, Is.Empty);
   }

   [Test]
   public void StressTest_ManyItems()
   {
      const int count = 1000;
      var rng = new Random(42);

      for (var i = 0; i < count; i++)
      {
         float x = rng.Next(0, 90);
         float y = rng.Next(0, 90);
         var ent = new TestEntity(x, y, 2, 2);
         _quadTree.Insert(ent);
      }

      var results = _quadTree.Query(_worldBounds);
      var uniqueIds = results.Select(e => e.Id).Distinct().Count();

      Assert.That(uniqueIds, Is.EqualTo(count));
   }
}