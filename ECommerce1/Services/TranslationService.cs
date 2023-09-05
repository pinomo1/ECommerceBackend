using ECommerce1.Extensions;
using ECommerce1.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace ECommerce1.Services
{

    public class TranslationService
    {
        private readonly ResourceDbContext resourceDbContext;

        public TranslationService(ResourceDbContext resourceDbContext)
        {
            this.resourceDbContext = resourceDbContext;
        }

        public async Task AddTranslation(TranslatedObjectType objType, string objId, string locale, string translation)
        {
            bool hasDefault = await resourceDbContext.Translations.AnyAsync(t => t.ObjectId == objId && t.ObjectType == objType && t.IsDefault);
            Translation? obj = await resourceDbContext.Translations.FirstOrDefaultAsync(t => t.ObjectId == objId && t.ObjectType == objType && t.Locale == locale);
            if (obj == null)
            {
                obj = new Translation()
                {
                    ObjectId = objId,
                    ObjectType = objType,
                    Locale = locale,
                    Text = translation,
                    IsDefault = !hasDefault
                };
                await resourceDbContext.Translations.AddAsync(obj);
            }
            else
            {
                obj.Text = translation;
            }
            await resourceDbContext.SaveChangesAsync();
        }

        public async Task<string> Translate(TranslatedObjectType objType, string objId, string locale)
        {
            Translation? translation = await resourceDbContext.Translations.FirstOrDefaultAsync(t => t.ObjectId == objId && t.ObjectType == objType && t.Locale == locale);
            translation ??= await resourceDbContext.Translations.FirstOrDefaultAsync(t => t.ObjectId == objId && t.ObjectType == objType && t.IsDefault);
            return translation != null ? translation.Text : "";
        }

        public async Task<string> Translate(TranslatedObjectType objType, string objId, HttpContext httpContext)
        {
            return await Translate(objType, objId, httpContext.GetLocale());
        }
    }
}
