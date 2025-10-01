using System.ComponentModel;
using System.Drawing.Design;

namespace SysBot.Pokemon.WinForms;

public class CollectionDescriptionProvider(Type type) : TypeDescriptionProvider(TypeDescriptor.GetProvider(type))
{
    private readonly TypeDescriptionProvider _baseProvider = TypeDescriptor.GetProvider(type);

    public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object? instance)
        => new DrawableCollectionEditorDescriptor(_baseProvider.GetTypeDescriptor(objectType, instance)!);
}

public class DrawableCollectionEditorDescriptor(ICustomTypeDescriptor parent) : CustomTypeDescriptor(parent)
{
    public override PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
    {
        var props = base.GetProperties(attributes);
        var list = new List<PropertyDescriptor>();
        foreach (PropertyDescriptor pd in props)
        {
            if (pd.PropertyType.IsGenericType &&
                pd.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                if (DrawableCollectionEditor.SupportedCollections.Contains(pd.PropertyType.GetGenericArguments()[0]))
                {
                    list.Add(new DrawableCollectionEditorPropertyDescriptor(pd));
                    continue;
                }
            }
            list.Add(pd);
        }
        return new PropertyDescriptorCollection([.. list]);
    }
}

public class DrawableCollectionEditorPropertyDescriptor(PropertyDescriptor baseProp) : PropertyDescriptor(baseProp)
{
    private readonly PropertyDescriptor _base = baseProp;

    public override Type ComponentType => _base.ComponentType;
    public override bool IsReadOnly => _base.IsReadOnly;
    public override Type PropertyType => _base.PropertyType;
    public override bool CanResetValue(object component) => _base.CanResetValue(component);
    public override object? GetValue(object? component) => _base.GetValue(component);
    public override void ResetValue(object component) => _base.ResetValue(component);
    public override void SetValue(object? component, object? value) => _base.SetValue(component, value);
    public override bool ShouldSerializeValue(object component) => _base.ShouldSerializeValue(component);

    public override object? GetEditor(Type editorBaseType)
    {
        if (editorBaseType == typeof(UITypeEditor))
            return new DrawableCollectionEditor(_base.PropertyType);
        return _base.GetEditor(editorBaseType);
    }
}
