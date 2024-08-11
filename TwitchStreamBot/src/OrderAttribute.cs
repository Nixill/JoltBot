using System.Reflection;

namespace Nixill.Streaming.JoltBot;

[AttributeUsage(AttributeTargets.All)]
public class OrderAttribute(int order) : Attribute
{
  public int Order = order;
}

public static class OrderAttributeExtensions
{
  public static IEnumerable<Assembly> OrderByAttribute(this IEnumerable<Assembly> asms)
    => asms.Order(AssemblyOrderAttributeComparer.Instance);

  public static IEnumerable<Module> OrderByAttribute(this IEnumerable<Module> mods)
    => mods.Order(ModuleOrderAttributeComparer.Instance);

  public static IEnumerable<MemberInfo> OrderByAttribute(this IEnumerable<MemberInfo> members)
    => members.Order(MemberInfoOrderAttributeComparer.Instance);
}

file class AssemblyOrderAttributeComparer : IComparer<Assembly>
{
  public int Compare(Assembly x, Assembly y)
  {
    var orderX = x.GetCustomAttribute<OrderAttribute>();
    var orderY = y.GetCustomAttribute<OrderAttribute>();

    if (orderX == null && orderY != null) return -1;
    if (orderX != null && orderY == null) return 1;
    if (orderX != null && orderY != null)
    {
      int compResult = orderX.Order.CompareTo(orderY.Order);
      if (compResult != 0) return compResult;
    }

    return x.FullName.CompareTo(y.FullName);
  }

  private AssemblyOrderAttributeComparer() { }

  public static AssemblyOrderAttributeComparer Instance = new();
}

file class ModuleOrderAttributeComparer : IComparer<Module>
{
  public int Compare(Module x, Module y)
  {
    var orderX = x.GetCustomAttribute<OrderAttribute>();
    var orderY = y.GetCustomAttribute<OrderAttribute>();

    if (orderX == null && orderY != null) return -1;
    if (orderX != null && orderY == null) return 1;
    if (orderX != null && orderY != null)
    {
      int compResult = orderX.Order.CompareTo(orderY.Order);
      if (compResult != 0) return compResult;
    }

    return x.Name.CompareTo(y.Name);
  }

  private ModuleOrderAttributeComparer() { }

  public static ModuleOrderAttributeComparer Instance = new();
}

file class MemberInfoOrderAttributeComparer : IComparer<MemberInfo>
{
  public int Compare(MemberInfo x, MemberInfo y)
  {
    var orderX = x.GetCustomAttribute<OrderAttribute>(true);
    var orderY = y.GetCustomAttribute<OrderAttribute>(true);

    if (orderX == null && orderY != null) return -1;
    if (orderX != null && orderY == null) return 1;
    if (orderX != null && orderY != null)
    {
      int compResult = orderX.Order.CompareTo(orderY.Order);
      if (compResult != 0) return compResult;
    }

    return x.Name.CompareTo(y.Name);
  }

  private MemberInfoOrderAttributeComparer() { }

  public static MemberInfoOrderAttributeComparer Instance = new();
}
