using GomokuWebAPI.Controllers;
using GomokuWebAPI.Model.Entities;
using GomokuWebAPI.Model.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace GomokuWebAPI.Common
{
    public static class ExtensionMethods
    {
        public static void SetSoftDeleteFilter(this ModelBuilder modelBuilder, string propertyName)
        {
            foreach (var type in modelBuilder.Model.GetEntityTypes())
            {
                var isActiveProperty = type.FindProperty(propertyName);
                if (isActiveProperty != null && isActiveProperty.ClrType == typeof(Status))
                {
                    var parameter = Expression.Parameter(type.ClrType, "p");
                    var property = Expression.PropertyOrField(parameter, nameof(Status));
                    var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(Status.Active)), parameter);

                    type.SetQueryFilter(filter);
                }
            }
        }

        public static List<PairStringShort> EnumToListPairs(this Type myEnum)
        {
            return Enum.GetNames(myEnum)
                .Select(x => new PairStringShort { Name = x, Id = (short)Enum.Parse(myEnum, x) })
                .ToList();
        }
    }
}
