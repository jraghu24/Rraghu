using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the Localization Service, which is an easy access to operations involving Languages and Dictionary
    /// </summary>
    public interface ILocalizationService : IService
    {
        //Possible to-do list:
        //Import DictionaryItem (?)
        //RemoveByLanguage (translations)
        //Add/Set Text (Insert/Update)
        //Remove Text (in translation)

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its <see cref="Int32"/> id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        IDictionaryItem GetDictionaryItemById(int id);

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its <see cref="Guid"/> id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        IDictionaryItem GetDictionaryItemById(Guid id);

        /// <summary>
        /// Gets a <see cref="IDictionaryItem"/> by its key
        /// </summary>
        /// <param name="key">Key of the <see cref="IDictionaryItem"/></param>
        /// <returns><see cref="IDictionaryItem"/></returns>
        IDictionaryItem GetDictionaryItemByKey(string key);

        /// <summary>
        /// Gets a list of children for a <see cref="IDictionaryItem"/>
        /// </summary>
        /// <param name="parentId">Id of the parent</param>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        IEnumerable<IDictionaryItem> GetDictionaryItemChildren(Guid parentId);

        /// <summary>
        /// Gets the root/top <see cref="IDictionaryItem"/> objects
        /// </summary>
        /// <returns>An enumerable list of <see cref="IDictionaryItem"/> objects</returns>
        IEnumerable<IDictionaryItem> GetRootDictionaryItems();

        /// <summary>
        /// Checks if a <see cref="IDictionaryItem"/> with given key exists
        /// </summary>
        /// <param name="key">Key of the <see cref="IDictionaryItem"/></param>
        /// <returns>True if a <see cref="IDictionaryItem"/> exists, otherwise false</returns>
        bool DictionaryItemExists(string key);

        /// <summary>
        /// Saves a <see cref="IDictionaryItem"/> object
        /// </summary>
        /// <param name="dictionaryItem"><see cref="IDictionaryItem"/> to save</param>
        void Save(IDictionaryItem dictionaryItem);

        /// <summary>
        /// Deletes a <see cref="IDictionaryItem"/> object and its related translations
        /// as well as its children.
        /// </summary>
        /// <param name="dictionaryItem"><see cref="IDictionaryItem"/> to delete</param>
        void Delete(IDictionaryItem dictionaryItem);

        /// <summary>
        /// Gets a <see cref="ILanguage"/> by its id
        /// </summary>
        /// <param name="id">Id of the <see cref="ILanguage"/></param>
        /// <returns><see cref="ILanguage"/></returns>
        ILanguage GetLanguageById(int id);

        /// <summary>
        /// Gets a <see cref="ILanguage"/> by its culture code
        /// </summary>
        /// <param name="culture">Culture Code</param>
        /// <returns><see cref="ILanguage"/></returns>
        ILanguage GetLanguageByCultureCode(string culture);

        /// <summary>
        /// Gets all available languages
        /// </summary>
        /// <returns>An enumerable list of <see cref="ILanguage"/> objects</returns>
        IEnumerable<ILanguage> GetAllLanguages();

        /// <summary>
        /// Saves a <see cref="ILanguage"/> object
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to save</param>
        void Save(ILanguage language);

        /// <summary>
        /// Deletes a <see cref="ILanguage"/> by removing it and its usages from the db
        /// </summary>
        /// <param name="language"><see cref="ILanguage"/> to delete</param>
        void Delete(ILanguage language);
    }
}