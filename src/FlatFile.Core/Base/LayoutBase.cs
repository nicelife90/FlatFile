﻿namespace FlatFile.Core.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using FlatFile.Core.Extensions;

    public abstract class LayoutBase<TTarget, TFieldSettings, TConstructor, TLayout>
        : ILayout<TTarget, TFieldSettings, TConstructor, TLayout>
        where TFieldSettings : FieldSettingsBase
        where TConstructor : IFieldSettingsConstructor<TFieldSettings, TConstructor> 
        where TLayout : ILayout<TTarget, TFieldSettings, TConstructor, TLayout>
    {
        private readonly IFieldsContainer<TFieldSettings> _fieldsContainer;
        private readonly IFieldSettingsFactory<TFieldSettings, TConstructor> _fieldSettingsFactory;
        private readonly IFieldSettingsBuilder<TFieldSettings, TConstructor> _builder;

        protected LayoutBase(
            IFieldSettingsFactory<TFieldSettings, TConstructor> fieldSettingsFactory, 
            IFieldSettingsBuilder<TFieldSettings, TConstructor> builder, 
            IFieldsContainer<TFieldSettings> fieldsContainer)
        {
            this._fieldSettingsFactory = fieldSettingsFactory;
            this._builder = builder;
            this._fieldsContainer = fieldsContainer;
        }

        protected virtual void ProcessProperty<TProperty>(Expression<Func<TTarget, TProperty>> expression, Action<TConstructor> settings)
        {
            var propertyInfo = GetPropertyInfo(expression);

            var constructor = _fieldSettingsFactory.CreateFieldSettings(propertyInfo);

            if (settings != null)
            {
                settings(constructor);
            }

            var fieldSettings = _builder.BuildSettings(constructor);

            _fieldsContainer.AddOrUpdate(fieldSettings);
        }

        protected virtual PropertyInfo GetPropertyInfo<TProperty>(Expression<Func<TTarget, TProperty>> expression)
        {
            var propertyInfo = expression.GetPropertyInfo();
            return propertyInfo;
        }

        protected virtual void MapLayout()
        {
        }

        public abstract TLayout WithMember<TProperty>(Expression<Func<TTarget, TProperty>> expression, Action<TConstructor> settings = null);
        public abstract TLayout WithHeader();

        public IEnumerable<TFieldSettings> Fields
        {
            get { return _fieldsContainer.OrderedFields; }
        }

        public bool HasHeader { get; protected set; }
    }
}