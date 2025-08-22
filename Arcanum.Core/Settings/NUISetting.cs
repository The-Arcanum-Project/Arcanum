using Arcanum.Core.CoreSystems.NUI;
using Arcanum.Core.GameObjects.Pops;

namespace Arcanum.Core.Settings;

public class NUISettings
{
   public NUISetting PopSettings { get; set; } = new(Pop.Field.Type,
                                                     Enum.GetValues<Pop.Field>().Cast<Enum>().ToArray(),
                                                     Enum.GetValues<Pop.Field>().Cast<Enum>().ToArray(),
                                                     Enum.GetValues<Pop.Field>().Cast<Enum>().ToArray(),
                                                     Enum.GetValues<Pop.Field>().Cast<Enum>().ToArray());
   public NUISetting PopTypeSettings { get; set; } = new(PopType.Field.Name,
                                                         Enum.GetValues<PopType.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<PopType.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<PopType.Field>().Cast<Enum>().ToArray(),
                                                         Enum.GetValues<PopType.Field>().Cast<Enum>().ToArray());
}