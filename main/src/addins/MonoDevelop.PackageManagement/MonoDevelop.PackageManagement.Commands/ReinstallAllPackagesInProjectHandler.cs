﻿//
// ReinstallAllPackagesInProjectHandler.cs
//
// Author:
//       Matt Ward <matt.ward@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.PackageManagement;
using MonoDevelop.Components.Commands;
using MonoDevelop.PackageManagement.NodeBuilders;
using MonoDevelop.Projects;

namespace MonoDevelop.PackageManagement.Commands
{
	public class ReinstallAllPackagesInProjectHandler : PackagesCommandHandler
	{
		protected override void Run ()
		{
			try {
				IPackageManagementProject project = PackageManagementServices.Solution.GetActiveProject ();
				var reinstallAction = new ReinstallProjectPackagesAction (project, PackageManagementServices.PackageManagementEvents);
				ProgressMonitorStatusMessage progressMessage = CreateProgressMessage (reinstallAction);
				PackageManagementServices.BackgroundPackageActionRunner.Run (progressMessage, reinstallAction);
			} catch (Exception ex) {
				ShowStatusBarError (ex);
			}
		}

		ProgressMonitorStatusMessage CreateProgressMessage (ReinstallProjectPackagesAction reinstallAction)
		{
			if (reinstallAction.Packages.Count () == 1) {
				return ProgressMonitorStatusMessageFactory.CreateReinstallingSinglePackageMessage (reinstallAction.Packages.First ().Id);
			}
			return ProgressMonitorStatusMessageFactory.CreateReinstallingPackagesInProjectMessage (reinstallAction.Packages.Count ());
		}

		void ShowStatusBarError (Exception ex)
		{
			ProgressMonitorStatusMessage message = ProgressMonitorStatusMessageFactory.CreateUpdatingPackagesInProjectMessage ();
			PackageManagementServices.BackgroundPackageActionRunner.ShowError (message, ex);
		}

		protected override void Update (CommandInfo info)
		{
			info.Visible = SelectedDotNetProjectHasPackagesRequiringReinstall ();
		}

		bool SelectedDotNetProjectHasPackagesRequiringReinstall ()
		{
			DotNetProject project = GetSelectedDotNetProject ();
			if (project == null)
				return false;

			var packageReferenceFile = new ProjectPackageReferenceFile (project);
			return packageReferenceFile.AnyPackagesToBeReinstalled ();
		}
	}
}
