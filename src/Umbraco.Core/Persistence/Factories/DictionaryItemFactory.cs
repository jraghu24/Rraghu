﻿using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Factories
{
    internal class DictionaryItemFactory : IEntityFactory<IDictionaryItem, DictionaryDto>
    {
        #region Implementation of IEntityFactory<DictionaryItem,DictionaryDto>

        public IDictionaryItem BuildEntity(DictionaryDto dto)
        {
            return new DictionaryItem(dto.Parent, dto.Key)
                       {
                           Id = dto.PrimaryKey, 
                           Key = dto.Id
                       };
        }

        public DictionaryDto BuildDto(IDictionaryItem entity)
        {
            return new DictionaryDto
                       {
                           Id = entity.Key,
                           Key = entity.ItemKey,
                           Parent = entity.ParentId,
                           PrimaryKey = entity.Id,
                           LanguageTextDtos = BuildLanguageTextDtos(entity)
                       };
        }

        #endregion

        private List<LanguageTextDto> BuildLanguageTextDtos(IDictionaryItem entity)
        {
            var list = new List<LanguageTextDto>();
            foreach (var translation in entity.Translations)
            {
                var text = new LanguageTextDto
                               {
                                   LanguageId = translation.Language.Id,
                                   UniqueId = translation.Key,
                                   Value = translation.Value
                               };

                if (translation.HasIdentity)
                    text.PrimaryKey = translation.Id;

                list.Add(text);
            }
            return list;
        }
    }
}