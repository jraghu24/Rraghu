using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the ContentService, which is an easy access to operations involving <see cref="IContent"/>
    /// </summary>
    public interface IContentService : IService
    {
        /// <summary>
        /// Creates an <see cref="IContent"/> object using the alias of the <see cref="IContentType"/>
        /// that this Content is based on.
        /// </summary>
        /// <param name="name">Name of the Content object</param>
        /// <param name="parentId">Id of Parent for the new Content</param>
        /// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional id of the user creating the content</param>
        /// <returns><see cref="IContent"/></returns>
        IContent CreateContent(string name, int parentId, string contentTypeAlias, int userId = 0);

        /// <summary>
        /// Creates an <see cref="IContent"/> object using the alias of the <see cref="IContentType"/>
        /// that this Content is based on.
        /// </summary>
        /// <param name="name">Name of the Content object</param>
        /// <param name="parent">Parent <see cref="IContent"/> object for the new Content</param>
        /// <param name="contentTypeAlias">Alias of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional id of the user creating the content</param>
        /// <returns><see cref="IContent"/></returns>
        IContent CreateContent(string name, IContent parent, string contentTypeAlias, int userId = 0);

        /// <summary>
        /// Gets an <see cref="IContent"/> object by Id
        /// </summary>
        /// <param name="id">Id of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        IContent GetById(int id);

        /// <summary>
        /// Gets an <see cref="IContent"/> object by its 'UniqueId'
        /// </summary>
        /// <param name="key">Guid key of the Content to retrieve</param>
        /// <returns><see cref="IContent"/></returns>
        IContent GetById(Guid key);

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by the Id of the <see cref="IContentType"/>
        /// </summary>
        /// <param name="id">Id of the <see cref="IContentType"/></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetContentOfContentType(int id);

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Level
        /// </summary>
        /// <param name="level">The level to retrieve Content from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetByLevel(int level);

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Children from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetChildren(int id);

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects versions by its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetVersions(int id);

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which reside at the first level / root
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetRootContent();

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has an expiration date greater then today
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetContentForExpiration();

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects, which has a release date greater then today
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetContentForRelease();

        /// <summary>
        /// Gets a collection of an <see cref="IContent"/> objects, which resides in the Recycle Bin
        /// </summary>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetContentInRecycleBin();

        /// <summary>
        /// Saves a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        void Save(IContent content, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Saves a collection of <see cref="IContent"/> objects.
        /// </summary>        
        /// <param name="contents">Collection of <see cref="IContent"/> to save</param>
        /// <param name="userId">Optional Id of the User saving the Content</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise events.</param>
        void Save(IEnumerable<IContent> contents, int userId = 0, bool raiseEvents = true);		

        /// <summary>
        /// Deletes all content of specified type. All children of deleted content is moved to Recycle Bin.
        /// </summary>
        /// <remarks>This needs extra care and attention as its potentially a dangerous and extensive operation</remarks>
        /// <param name="contentTypeId">Id of the <see cref="IContentType"/></param>
        /// <param name="userId">Optional Id of the user issueing the delete operation</param>
        void DeleteContentOfType(int contentTypeId, int userId = 0);

        /// <summary>
        /// Permanently deletes versions from an <see cref="IContent"/> object prior to a specific date.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> object to delete versions from</param>
        /// <param name="versionDate">Latest version date</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        void DeleteVersions(int id, DateTime versionDate, int userId = 0);

        /// <summary>
        /// Permanently deletes a specific version from an <see cref="IContent"/> object.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> object to delete a version from</param>
        /// <param name="versionId">Id of the version to delete</param>
        /// <param name="deletePriorVersions">Boolean indicating whether to delete versions prior to the versionId</param>
        /// <param name="userId">Optional Id of the User deleting versions of a Content object</param>
        void DeleteVersion(int id, Guid versionId, bool deletePriorVersions, int userId = 0);

        /// <summary>
        /// Deletes an <see cref="IContent"/> object by moving it to the Recycle Bin
        /// </summary>
        /// <remarks>Move an item to the Recycle Bin will result in the item being unpublished</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        void MoveToRecycleBin(IContent content, int userId = 0);

        /// <summary>
        /// Moves an <see cref="IContent"/> object to a new location
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to move</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="userId">Optional Id of the User moving the Content</param>
        void Move(IContent content, int parentId, int userId = 0);

        /// <summary>
        /// Empties the Recycle Bin by deleting all <see cref="IContent"/> that resides in the bin
        /// </summary>
        void EmptyRecycleBin();

        /// <summary>
        /// Rollback an <see cref="IContent"/> object to a previous version.
        /// This will create a new version, which is a copy of all the old data.
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/>being rolled back</param>
        /// <param name="versionId">Id of the version to rollback to</param>
        /// <param name="userId">Optional Id of the User issueing the rollback of the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        IContent Rollback(int id, Guid versionId, int userId = 0);

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by its name or partial name
        /// </summary>
        /// <param name="parentId">Id of the Parent to retrieve Children from</param>
        /// <param name="name">Full or partial name of the children</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetChildrenByName(int parentId, string name);

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="id">Id of the Parent to retrieve Descendants from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetDescendants(int id);

        /// <summary>
        /// Gets a collection of <see cref="IContent"/> objects by Parent Id
        /// </summary>
        /// <param name="content"><see cref="IContent"/> item to retrieve Descendants from</param>
        /// <returns>An Enumerable list of <see cref="IContent"/> objects</returns>
        IEnumerable<IContent> GetDescendants(IContent content);

        /// <summary>
        /// Gets a specific version of an <see cref="IContent"/> item.
        /// </summary>
        /// <param name="versionId">Id of the version to retrieve</param>
        /// <returns>An <see cref="IContent"/> item</returns>
        IContent GetByVersion(Guid versionId);

        /// <summary>
        /// Gets the published version of an <see cref="IContent"/> item
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/> to retrieve version from</param>
        /// <returns>An <see cref="IContent"/> item</returns>
        IContent GetPublishedVersion(int id);

        /// <summary>
        /// Checks whether an <see cref="IContent"/> item has any children
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/></param>
        /// <returns>True if the content has any children otherwise False</returns>
        bool HasChildren(int id);

        /// <summary>
        /// Checks whether an <see cref="IContent"/> item has any published versions
        /// </summary>
        /// <param name="id">Id of the <see cref="IContent"/></param>
        /// <returns>True if the content has any published version otherwise False</returns>
        bool HasPublishedVersion(int id);

        /// <summary>
        /// Re-Publishes all Content
        /// </summary>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        bool RePublishAll(int userId = 0);

        /// <summary>
        /// Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        bool Publish(IContent content, int userId = 0);

        /// <summary>
        /// Publishes a <see cref="IContent"/> object and all its children
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish along with its children</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        bool PublishWithChildren(IContent content, int userId = 0);

        /// <summary>
        /// UnPublishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <returns>True if unpublishing succeeded, otherwise False</returns>
        bool UnPublish(IContent content, int userId = 0);

        /// <summary>
        /// Saves and Publishes a single <see cref="IContent"/> object
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to save and publish</param>
        /// <param name="userId">Optional Id of the User issueing the publishing</param>
        /// <param name="raiseEvents">Optional boolean indicating whether or not to raise save events.</param>
        /// <returns>True if publishing succeeded, otherwise False</returns>
        bool SaveAndPublish(IContent content, int userId = 0, bool raiseEvents = true);

        /// <summary>
        /// Permanently deletes an <see cref="IContent"/> object.
        /// </summary>
        /// <remarks>
        /// This method will also delete associated media files, child content and possibly associated domains.
        /// </remarks>
        /// <remarks>Please note that this method will completely remove the Content from the database</remarks>
        /// <param name="content">The <see cref="IContent"/> to delete</param>
        /// <param name="userId">Optional Id of the User deleting the Content</param>
        void Delete(IContent content, int userId = 0);

        /// <summary>
        /// Copies an <see cref="IContent"/> object by creating a new Content object of the same type and copies all data from the current 
        /// to the new copy which is returned.
        /// </summary>
        /// <param name="content">The <see cref="IContent"/> to copy</param>
        /// <param name="parentId">Id of the Content's new Parent</param>
        /// <param name="relateToOriginal">Boolean indicating whether the copy should be related to the original</param>
        /// <param name="userId">Optional Id of the User copying the Content</param>
        /// <returns>The newly created <see cref="IContent"/> object</returns>
        IContent Copy(IContent content, int parentId, bool relateToOriginal, int userId = 0);

        /// <summary>
        /// Checks if the passed in <see cref="IContent"/> can be published based on the anscestors publish state.
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to check if anscestors are published</param>
        /// <returns>True if the Content can be published, otherwise False</returns>
        bool IsPublishable(IContent content);
    }
}