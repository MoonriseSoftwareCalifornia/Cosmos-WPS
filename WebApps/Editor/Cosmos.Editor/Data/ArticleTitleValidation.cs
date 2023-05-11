using Cosmos.Common.Data;
using Cosmos.Common.Data.Logic;
using Cosmos.Cms.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Cosmos.Cms.Data
{
    /// <summary>
    ///     Validates that a title is valid.
    /// </summary>
    /// <remarks>
    ///     <para>This validator checks for the following:</para>
    ///     <list type="bullet">
    ///         <item>That the title is not null or empty space.</item>
    ///         <item>Ensures the title must be unique.</item>
    ///         <item>Prevents titles from being named "root," which is a key word.</item>
    ///     </list>
    ///     <para>Note: This validator will return invalid if it cannot connect to the <see cref="ApplicationDbContext" />.</para>
    /// </remarks>
    public class ArticleTitleValidation : ValidationAttribute
    {
        /// <summary>
        ///     Validates the current value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //
            // Make sure it doesn't conflict with the public blob path
            //

            if (value == null) return new ValidationResult("Title cannot be null or empty.");

            if (validationContext == null) return new ValidationResult("ValidationResult cannot be null or empty.");

            var title = value.ToString()?.ToLower().Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrWhiteSpace(title))
                return new ValidationResult("Title cannot be an empty string.");

            if (title == "root") return new ValidationResult("Cannot name an article with the name \"root.\"");

            var dbContext = (ApplicationDbContext)validationContext
                .GetService(typeof(ApplicationDbContext));

            if (dbContext == null) throw new Exception("Validator could not connect to ApplicationDbContext.");

            var setting = dbContext.Settings.FirstOrDefaultAsync(f => f.Name == "ReservedPaths").Result;

            if (setting != null)
            {
                var paths = JsonConvert.DeserializeObject<List<ReservedPath>>(setting.Value);

                foreach (var item in paths)
                {
                    if (item.Path.EndsWith("*"))
                    {
                        var wild = item.Path.TrimEnd(new char[] { '*' }).ToLower();
                        if (title.StartsWith(wild))
                        {
                            return new ValidationResult($"'{value.ToString()}' conflicts with a reserved path.");
                        }
                    }
                    else if (title == item.Path.ToLower())
                    {
                        return new ValidationResult($"'{value.ToString()}' conflicts with a reserved path.");
                    }
                }

            }

            var property = validationContext.ObjectType.GetProperty("ArticleNumber");

            if (property == null) throw new Exception("Validator could not connect to ArticleNumber property.");

            var articleNumber = (int)property.GetValue(validationContext.ObjectInstance, null);

            var result = dbContext.Articles.Where(a =>
                a.Title.ToLower() == title &&
                a.ArticleNumber != articleNumber &&
                a.StatusCode != (int)StatusCodeEnum.Deleted &&
                a.StatusCode != (int)StatusCodeEnum.Redirect).CountAsync().Result;

            if (result > 0)
                return new ValidationResult($"'{value.ToString()}' is already taken.");

            return ValidationResult.Success;
        }
    }
}