using MedEasy.DTO;
using System;

namespace MedEasy.Queries.Document
{
    /// <summary>
    /// Gets a <see cref="DocumentMetadataInfo"/> by its <see cref="DocumentMetadataInfo.Id"/>
    /// </summary>
    public interface IWantOneDocumentMetadataInfoByIdQuery : IWantOneResource<Guid, Guid, DocumentMetadataInfo>
    { 
    }


    
}
