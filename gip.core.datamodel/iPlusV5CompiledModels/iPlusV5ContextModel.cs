﻿// <auto-generated />
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

#pragma warning disable 219, 612, 618
#nullable disable

namespace gip.core.datamodel
{
    [DbContext(typeof(iPlusV5Context))]
    public partial class iPlusV5ContextModel : RuntimeModel
    {
        static iPlusV5ContextModel()
        {
            var model = new iPlusV5ContextModel();
            model.Initialize();
            model.Customize();
            _instance = model;
        }

        private static iPlusV5ContextModel _instance;
        public static IModel Instance => _instance;

        partial void Initialize();

        partial void Customize();
    }
}
