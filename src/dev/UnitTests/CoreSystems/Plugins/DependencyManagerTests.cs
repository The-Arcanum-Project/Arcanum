using System.Reflection;
using Arcanum.API;
using Arcanum.Core.CoreSystems.PluginServices;
using Moq;

namespace UnitTests.CoreSystems.Plugins;

public class DependencyManagerTests
{
   [Test]
   public void TopologicalSort_ResolvesOrderCorrectly()
   {
      var a = MockPlugin("A");
      var b = MockPlugin("B", [a]);
      var c = MockPlugin("C", [b]);
      var d = MockPlugin("D", [a, c]);

      var nodes = new Dictionary<Guid, DependencyManager.PluginNode>
      {
         [a.Guid] = new(a),
         [b.Guid] = new(b),
         [c.Guid] = new(c),
         [d.Guid] = new(d),
      };

      var sorted = DependencyManager.TopologicalSort(nodes);

      // Valid orders: A before B, B before C, C before D, A before D
      Assert.That(sorted.IndexOf(a), Is.LessThan(sorted.IndexOf(b)));
      Assert.That(sorted.IndexOf(b), Is.LessThan(sorted.IndexOf(c)));
      Assert.That(sorted.IndexOf(c), Is.LessThan(sorted.IndexOf(d)));
      Assert.That(sorted.IndexOf(a), Is.LessThan(sorted.IndexOf(d)));
   }

   [Test]
   public void TopologicalSort_ThrowsOnMissingDependency()
   {
      var a = MockPlugin("A");
      var b = MockPlugin("B", Guid.NewGuid()); // Non-existent dependency

      var nodes = new Dictionary<Guid, DependencyManager.PluginNode> { [a.Guid] = new(a), [b.Guid] = new(b) };

      Assert.Throws<Exception>(() => DependencyManager.TopologicalSort(nodes));
   }

   [Test]
   public void TopologicalSort_ThrowsOnCircularDependency()
   {
      var aGuid = Guid.NewGuid();
      var bGuid = Guid.NewGuid();
      var cGuid = Guid.NewGuid();
      var a = MockPluginWithGuid("A", aGuid, bGuid);
      var b = MockPluginWithGuid("B", bGuid, cGuid);

      var nodes = new Dictionary<Guid, DependencyManager.PluginNode> { [a.Guid] = new(a), [b.Guid] = new(b) };

      Assert.Throws<Exception>(() => DependencyManager.TopologicalSort(nodes));
   }

   static IPluginMetadata MockPlugin(string name, IPluginMetadata[] deps)
   {
      return MockPlugin(name, deps.Select(d => d.Guid).ToArray());
   }

   static IPluginMetadata MockPlugin(string name, params Guid[] depGuids)
   {
      var mock = new Mock<IPluginMetadata>();
      mock.Setup(x => x.Name).Returns(name);
      mock.Setup(x => x.Guid).Returns(Guid.NewGuid());
      mock.Setup(x => x.Dependencies)
          .Returns(depGuids.Select(g => new IPluginMetadata.PluginDependency(g, new(1, 1))).ToList());
      return mock.Object;
   }

   static IPluginMetadata MockPluginWithGuid(string name, Guid guid, params Guid[] depGuids)
   {
      var mock = new Mock<IPluginMetadata>();
      mock.Setup(x => x.Name).Returns(name);
      mock.Setup(x => x.Guid).Returns(guid);
      mock.Setup(x => x.Dependencies)
          .Returns(depGuids.Select(g => new IPluginMetadata.PluginDependency(g, new(1, 1))).ToList());
      return mock.Object;
   }

   private Mock<IPluginMetadata> CreatePlugin(Guid guid, params Guid[] dependencies)
   {
      var mock = new Mock<IPluginMetadata>();
      mock.Setup(x => x.Guid).Returns(guid);
      mock.Setup(x => x.Dependencies)
          .Returns(dependencies.Select(g => new IPluginMetadata.PluginDependency(g, new(1, 1))).ToList());
      return mock;
   }

   [Test]
   public void GetAllDependentFor_ReturnsAllDependenciesInOrder()
   {
      var guidA = Guid.NewGuid();
      var guidB = Guid.NewGuid();
      var guidC = Guid.NewGuid();

      var pluginA = CreatePlugin(guidA).Object;
      var pluginB = CreatePlugin(guidB, guidA).Object;
      var pluginC = CreatePlugin(guidC, guidB).Object;

      var allPlugins = new[] { pluginA, pluginB, pluginC };

      var result = DependencyManager.GetAllDependentFor(pluginC, allPlugins);

      Assert.That(result, Is.EquivalentTo(new[] { pluginC, pluginB, pluginA }));
   }

   [Test]
   public void GetAllDependentOn_ReturnsAllDependents()
   {
      var guidA = Guid.NewGuid();
      var guidB = Guid.NewGuid();
      var guidC = Guid.NewGuid();

      var pluginA = CreatePlugin(guidA).Object;
      var pluginB = CreatePlugin(guidB, guidA).Object;
      var pluginC = CreatePlugin(guidC, guidB).Object;

      var allPlugins = new[] { pluginA, pluginB, pluginC };

      var result = DependencyManager.GetAllDependentOn(pluginA, allPlugins);

      Assert.That(result, Is.EquivalentTo(new[] { pluginA, pluginB, pluginC }));
   }

   [Test]
   public void GetAllDependentFor_PluginWithoutDependencies_ReturnsOnlySelf()
   {
      var guid = Guid.NewGuid();
      var plugin = CreatePlugin(guid).Object;

      var result = DependencyManager.GetAllDependentFor(plugin, [plugin]);

      Assert.That(result, Is.EquivalentTo(new[] { plugin }));
   }

   [Test]
   public void DesensitizePath_StripsAppPath()
   {
      var hostMock = new Mock<IPluginHost>();
      var manager = new PluginManager(hostMock.Object);

      var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Test.dll");
      var result = manager.GetType().GetMethod("DesensitizePath", BindingFlags.NonPublic | BindingFlags.Instance)!
                          .Invoke(manager, [path]);

      Assert.That(result, Is.EqualTo("'Arcanum/Plugins\\Test.dll'"));
   }
}