using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Auditing;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using umbraco.interfaces;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the DataType Service, which is an easy access to operations involving <see cref="IDataType"/> and <see cref="IDataTypeDefinition"/>
    /// </summary>
    public class DataTypeService : IDataTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
		private readonly IDataTypeDefinitionRepository _dataTypeService;
	    private readonly IContentTypeRepository _contentTypeRepository;

        public DataTypeService() : this(new PetaPocoUnitOfWorkProvider())
        {
        }

        public DataTypeService(IUnitOfWorkProvider provider)
        {
            _unitOfWork = provider.GetUnitOfWork();
	        _dataTypeService = RepositoryResolver.Current.Factory.CreateDataTypeDefinitionRepository(_unitOfWork);
            _contentTypeRepository = RepositoryResolver.Current.Factory.CreateContentTypeRepository(_unitOfWork);
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its Id
        /// </summary>
        /// <param name="id">Id of the <see cref="IDataTypeDefinition"/></param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(int id)
        {
            var repository = _dataTypeService;
            return repository.Get(id);
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its unique guid Id
        /// </summary>
        /// <param name="id">Unique guid Id of the DataType</param>
        /// <returns><see cref="IDataTypeDefinition"/></returns>
        public IDataTypeDefinition GetDataTypeDefinitionById(Guid id)
        {
            var repository = _dataTypeService;

            var query = Query<IDataTypeDefinition>.Builder.Where(x => x.Key == id);
            var definitions = repository.GetByQuery(query);

            return definitions.FirstOrDefault();
        }

        /// <summary>
        /// Gets a <see cref="IDataTypeDefinition"/> by its control Id
        /// </summary>
        /// <param name="id">Id of the DataType control</param>
        /// <returns>Collection of <see cref="IDataTypeDefinition"/> objects with a matching contorl id</returns>
        public IEnumerable<IDataTypeDefinition> GetDataTypeDefinitionByControlId(Guid id)
        {
            var repository = _dataTypeService;

            var query = Query<IDataTypeDefinition>.Builder.Where(x => x.ControlId == id);
            var definitions = repository.GetByQuery(query);

            return definitions;
        }

        /// <summary>
        /// Gets all <see cref="IDataTypeDefinition"/> objects or those with the ids passed in
        /// </summary>
        /// <param name="ids">Optional array of Ids</param>
        /// <returns>An enumerable list of <see cref="IDataTypeDefinition"/> objects</returns>
        public IEnumerable<IDataTypeDefinition> GetAllDataTypeDefinitions(params int[] ids)
        {
            var repository = _dataTypeService;
            return repository.GetAll(ids);
        }

        /// <summary>
        /// Saves an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <param name="dataTypeDefinition"><see cref="IDataTypeDefinition"/> to save</param>
        /// <param name="userId">Id of the user issueing the save</param>
        public void Save(IDataTypeDefinition dataTypeDefinition, int userId = -1)
        {
            var e = new SaveEventArgs();
            if (Saving != null)
                Saving(dataTypeDefinition, e);

            if (!e.Cancel)
            {
                dataTypeDefinition.CreatorId = userId > -1 ? userId : 0;
                _dataTypeService.AddOrUpdate(dataTypeDefinition);
                _unitOfWork.Commit();

                if (Saved != null)
                    Saved(dataTypeDefinition, e);

                Audit.Add(AuditTypes.Save, string.Format("Save DataTypeDefinition performed by user"), userId == -1 ? 0 : userId, dataTypeDefinition.Id);
            }
        }

        /// <summary>
        /// Deletes an <see cref="IDataTypeDefinition"/>
        /// </summary>
        /// <remarks>
        /// Please note that deleting a <see cref="IDataTypeDefinition"/> will remove
        /// all the <see cref="PropertyType"/> data that references this <see cref="IDataTypeDefinition"/>.
        /// </remarks>
        /// <param name="dataTypeDefinition"><see cref="IDataTypeDefinition"/> to delete</param>
        /// <param name="userId">Optional Id of the user issueing the deletion</param>
        public void Delete(IDataTypeDefinition dataTypeDefinition, int userId = -1)
        {
            var e = new DeleteEventArgs { Id = dataTypeDefinition.Id };
            if (Deleting != null)
                Deleting(dataTypeDefinition, e);

            if (!e.Cancel)
            {
                //Find ContentTypes using this IDataTypeDefinition on a PropertyType
                var contentTypeRepository = _contentTypeRepository;
                var query = Query<PropertyType>.Builder.Where(x => x.DataTypeId == dataTypeDefinition.Id);
                var contentTypes = contentTypeRepository.GetByQuery(query);

                //Loop through the list of results and remove the PropertyTypes that references the DataTypeDefinition that is being deleted
                foreach (var contentType in contentTypes)
                {
                    if (contentType == null) continue;

                    foreach (var group in contentType.PropertyGroups)
                    {
                        var types = group.PropertyTypes.Where(x => x.DataTypeId == dataTypeDefinition.Id);
                        foreach (var propertyType in types)
                        {
                            group.PropertyTypes.Remove(propertyType);
                        }
                    }

                    contentTypeRepository.AddOrUpdate(contentType);
                }

                var repository = _dataTypeService;
                repository.Delete(dataTypeDefinition);
                _unitOfWork.Commit();

                if (Deleted != null)
                    Deleted(dataTypeDefinition, e);

                Audit.Add(AuditTypes.Delete, string.Format("Delete DataTypeDefinition performed by user"), userId == -1 ? 0 : userId, dataTypeDefinition.Id);
            }
        }

        /// <summary>
        /// Gets the <see cref="IDataType"/> specified by it's unique ID
        /// </summary>
        /// <param name="id">Id of the DataType, which corresponds to the Guid Id of the control</param>
        /// <returns><see cref="IDataType"/> object</returns>
        public IDataType GetDataTypeById(Guid id)
        {
            return DataTypesResolver.Current.GetById(id);
        }

        /// <summary>
        /// Gets a complete list of all registered <see cref="IDataType"/>'s
        /// </summary>
        /// <returns>An enumerable list of <see cref="IDataType"/> objects</returns>
        public IEnumerable<IDataType> GetAllDataTypes()
        {
            return DataTypesResolver.Current.DataTypes;
        }

        #region Event Handlers
        /// <summary>
        /// Occurs before Delete
        /// </summary>
        public static event EventHandler<DeleteEventArgs> Deleting;

        /// <summary>
        /// Occurs after Delete
        /// </summary>
        public static event EventHandler<DeleteEventArgs> Deleted;

        /// <summary>
        /// Occurs before Save
        /// </summary>
        public static event EventHandler<SaveEventArgs> Saving;

        /// <summary>
        /// Occurs after Save
        /// </summary>
        public static event EventHandler<SaveEventArgs> Saved;
        #endregion
    }
}