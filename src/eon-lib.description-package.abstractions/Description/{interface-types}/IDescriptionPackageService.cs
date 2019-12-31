using System.Collections.Generic;
using System.Threading.Tasks;

using Eon.Context;
using Eon.IO;
using Eon.Metadata;

namespace Eon.Description {
	using IDescriptionPackageMaterializer = ISubjectMaterializer<IDescriptionPackage>;

	/// <summary>
	/// Description package service.
	/// </summary>
	public interface IDescriptionPackageService
		:IDisposeNotifying {

		DescriptionPackageLocator CreateSatellitePackageLocator(DescriptionPackageLocator locator, MetadataName satelliteName);

		DescriptionLocator CreateSatelliteDescriptionLocator(DescriptionPackageLocator locator, MetadataName satelliteName, MetadataPathName satelliteDescriptionPath);

		DescriptionPackageLocator CreatePackageLocator(IDescriptionPackage package);

		Task<TDescription> LoadDescriptionAsync<TDescription>(DescriptionLocator locator, IContext ctx = default)
			where TDescription : class, IDescription;

		IDescriptionPackageLoadContext CreateLoadContext(DescriptionPackageLocator packageLocator, IContext outerCtx = default);

		IDescriptionPackageMaterializer CreateMaterializer(IDescriptionPackageLoadContext loadCtx);

		Task<IDescriptionPackage> CreateAndLoadAsync(IDescription description, bool ownsDescription, IContext ctx = default);

		Task<IDescriptionPackage> CreateAndLoadAsync(IEnumerable<IDescription> descriptions, bool ownsDescription, IContext ctx = default);

		Task<IDescriptionPackage> LoadAsync(DescriptionPackageLocator locator, IContext ctx = default);

		IDescriptionPackage[ ] GetLoaded();

	}

}