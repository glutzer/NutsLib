using MareLib;
using OpenTK.Mathematics;
using System;
using System.Reflection;

namespace Equimancy;

public class ParticleConfigGui : Gui
{
    public ParticleConfig currentConfig;
    public ParticleBlockEntity particleBE;

    public ParticleConfigGui(ParticleConfig currentConfig, ParticleBlockEntity particleBE) : base()
    {
        this.currentConfig = currentConfig;
        this.particleBE = particleBE;
    }

    public override void PopulateWidgets(out Bounds mainBounds)
    {
        mainBounds = Bounds.Create().Alignment(Align.RightTop).Fixed(0, 50, 150, 200);

        // Bg.
        AddWidget(new BackgroundWidgetSkia(this, mainBounds, EqGui.Background));

        Type type = typeof(ParticleConfig);

        // Go over each field in the type.
        int i = 0;

        Bounds scrollBounds = Bounds.CreateFrom(mainBounds).PercentWidth(1).Alignment(Align.CenterTop);
        Bounds barBounds = Bounds.CreateFrom(mainBounds).Percent(0, 0, 0.05f, 1f).Alignment(Align.LeftMiddle, true);

        Bounds titleBounds = Bounds.CreateFrom(mainBounds).PercentWidth(1).FixedHeight(7).Alignment(Align.CenterTop, false, true);
        AddWidget(new DraggableTitle(this, titleBounds, mainBounds, "Particle Config", 7));

        Bounds xBounds = Bounds.CreateFrom(barBounds).PercentWidth(1).FixedHeight(7).Alignment(Align.CenterTop, false, true);
        AddWidget(new ButtonWidget(this, xBounds, () => TryClose(), "X", Scaled(5)));

        Bounds setBounds = Bounds.CreateFrom(mainBounds).Fixed(0, 0, 40, 7).Alignment(Align.CenterBottom, false, true);
        AddWidget(new ButtonWidget(this, setBounds, () =>
        {
            particleBE.UpdateConfigFromClient();
        }, "Set", Scaled(6)));

        AddWidget(new ClipWidget(this, true, mainBounds));

        foreach (FieldInfo field in type.GetFields())
        {
            // Create a new bounds for the field.
            Bounds fieldBounds = Bounds.CreateFrom(scrollBounds).Fixed(0, i * 8, 200, 7).PercentWidth(0.5f).Alignment(Align.LeftTop);
            Bounds nameBounds = Bounds.CreateFrom(scrollBounds).Fixed(0, i * 8, 200, 7).PercentWidth(0.5f).PercentX(0.5f).Alignment(Align.LeftTop);
            int fontSize = Scaled(5);

            if (field.FieldType == typeof(float))
            {
                AddWidget(new InputBox(this, fieldBounds, 1, fontSize, (i, s) =>
                {
                    try
                    {
                        field.SetValue(currentConfig, float.Parse(s));
                    }
                    catch
                    {
                        field.SetValue(currentConfig, 1f);
                    }
                }, true, field.GetValue(currentConfig).ToString()));

                AddWidget(new SimpleTextWidget(this, nameBounds, field.Name, fontSize));

                i++;
            }

            if (field.FieldType == typeof(double))
            {
                AddWidget(new InputBox(this, fieldBounds, 1, fontSize, (i, s) =>
                {
                    try
                    {
                        field.SetValue(currentConfig, double.Parse(s));
                    }
                    catch
                    {
                        field.SetValue(currentConfig, 1d);
                    }
                }, true, field.GetValue(currentConfig).ToString()));

                AddWidget(new SimpleTextWidget(this, nameBounds, field.Name, fontSize));

                i++;
            }

            if (field.FieldType == typeof(int))
            {
                AddWidget(new InputBox(this, fieldBounds, 1, fontSize, (i, s) =>
                {
                    try
                    {
                        field.SetValue(currentConfig, int.Parse(s));
                    }
                    catch
                    {
                        field.SetValue(currentConfig, 1);
                    }
                }, true, field.GetValue(currentConfig).ToString()));

                AddWidget(new SimpleTextWidget(this, nameBounds, field.Name, fontSize));

                i++;
            }

            if (field.FieldType == typeof(Vector3))
            {
                Vector3 cValue = (Vector3)field.GetValue(currentConfig)!;

                AddWidget(new InputBox(this, fieldBounds, 3, fontSize, (i, s) =>
                {
                    try
                    {
                        float value = float.Parse(s);
                        Vector3 currentValue = (Vector3)field.GetValue(currentConfig)!;

                        if (i == 0) currentValue.X = value;
                        if (i == 1) currentValue.Y = value;
                        if (i == 2) currentValue.Z = value;

                        field.SetValue(currentConfig, currentValue);
                    }
                    catch
                    {

                    }
                }, true, cValue.X.ToString(), cValue.Y.ToString(), cValue.Z.ToString()));

                AddWidget(new SimpleTextWidget(this, nameBounds, field.Name, fontSize));

                i++;
            }

            if (field.FieldType == typeof(Vector4))
            {
                Vector4 cValue = (Vector4)field.GetValue(currentConfig)!;

                AddWidget(new InputBox(this, fieldBounds, 4, fontSize, (i, s) =>
                {
                    try
                    {
                        float value = float.Parse(s);
                        Vector4 currentValue = (Vector4)field.GetValue(currentConfig)!;
                        if (i == 0) currentValue.X = value;
                        if (i == 1) currentValue.Y = value;
                        if (i == 2) currentValue.Z = value;
                        if (i == 3) currentValue.W = value;
                        field.SetValue(currentConfig, currentValue);
                    }
                    catch
                    {

                    }
                }, true, cValue.X.ToString(), cValue.Y.ToString(), cValue.Z.ToString(), cValue.W.ToString()));

                AddWidget(new SimpleTextWidget(this, nameBounds, field.Name, fontSize));

                i++;
            }

            if (field.FieldType == typeof(string))
            {
                string defaultValue = field.GetValue(currentConfig) as string ?? "NULL";

                AddWidget(new InputBox(this, fieldBounds, 1, fontSize, (i, s) =>
                {
                    field.SetValue(currentConfig, s);
                }, false, defaultValue));

                AddWidget(new SimpleTextWidget(this, nameBounds, field.Name, fontSize));

                i++;
            }
        }

        scrollBounds.FixedHeight(i * 8);

        AddWidget(new ClipWidget(this, false, mainBounds));

        AddWidget(new ScrollBarWidget(this, barBounds, scrollBounds));
    }
}