using SoundbankEditor.Core;
using SoundbankEditor.Core.WwiseObjects.HircItems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SoundbankEditor.SpecificHircItemEditorViews
{
	/// <summary>
	/// Interaction logic for RandomSequenceContainerHircItemEditorView.xaml
	/// </summary>
	public partial class RandomSequenceContainerHircItemEditorView : UserControl
	{
		private CAkRanSeqCntr? _cakRanSeqCntr;

		public event EventHandler? HircItemUpdated;

		public RandomSequenceContainerHircItemEditorView()
		{
			InitializeComponent();
			MainWindow.OnHircItemUpdated += UpdateAllFields;
		}

		//
		// Event Handlers
		//

		private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_cakRanSeqCntr = (CAkRanSeqCntr)DataContext;

			if (_cakRanSeqCntr != null )
			{
				hiilc.Ids = _cakRanSeqCntr.CAkPlayList.PlaylistItems.Select(pi => pi.PlayId).ToList();
			}
			
			UpdateAllFields();
		}

		private void hiilc_IdsUpdated(object sender, EventArgs e)
		{
			if (_cakRanSeqCntr == null || hiilc.Ids == null)
			{
				return;
			}

			List<uint> addedIds = hiilc.Ids.Where(id => !_cakRanSeqCntr.CAkPlayList.PlaylistItems.Any(pi => pi.PlayId == id)).ToList();
			List<uint> removedIds = _cakRanSeqCntr.CAkPlayList.PlaylistItems.Where(pi => !hiilc.Ids.Contains(pi.PlayId)).Select(pi => pi.PlayId).ToList();

			_cakRanSeqCntr.CAkPlayList.PlaylistItems = hiilc.Ids.Select(id =>
			{
				AkPlaylistItem? akPlaylistItem = _cakRanSeqCntr.CAkPlayList.PlaylistItems.Find(pi => pi.PlayId == id);
				if (akPlaylistItem != null)
				{
					return akPlaylistItem;
				}

				return new AkPlaylistItem
				{
					PlayId = id,
					Weight = 50000,
				};
			}).ToList();

			foreach (uint nodeId in addedIds)
			{
				AddLinksToPlaylistItemIdIfNecessary(nodeId);
			}
			foreach (uint nodeId in removedIds)
			{
				RemoveLinksToPlaylistItemIdIfUnused(nodeId);
			}

			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		private void IfevEditDirectParentId_Click(object sender, EventArgs e)
		{
			if (_cakRanSeqCntr == null)
			{
				return;
			}

			var hircItemIdConverterWindow = new HircItemIdConverterWindow("Set Direct Parent ID", _cakRanSeqCntr.NodeBaseParams.DirectParentID);
			if (hircItemIdConverterWindow.ShowDialog() != true || hircItemIdConverterWindow.Id == null)
			{
				return;
			}

			_cakRanSeqCntr.NodeBaseParams.DirectParentID = hircItemIdConverterWindow.Id.Value;
			UpdateDirectParentIdTextBlock();
			HircItemUpdated?.Invoke(this, EventArgs.Empty);
		}

		//
		// Helpers
		//

		private void AddLinksToPlaylistItemIdIfNecessary(uint piId)
		{
			if (_cakRanSeqCntr == null)
			{
				return;
			}

			if (!_cakRanSeqCntr.ChildIds.Any(id => id == piId))
			{
				_cakRanSeqCntr.ChildIds.Add(piId);
				_cakRanSeqCntr.ChildIds.Sort((a, b) => WwiseShortIdUtility.CompareShortIds(a, b));
			}
		}

		private void RemoveLinksToPlaylistItemIdIfUnused(uint piId)
		{
			if (_cakRanSeqCntr == null)
			{
				return;
			}

			if (!_cakRanSeqCntr.CAkPlayList.PlaylistItems.Any(pi => pi.PlayId == piId))
			{
				_cakRanSeqCntr.ChildIds.RemoveAll(id => id == piId);
			}
		}

		private void UpdateAllFields()
		{
			UpdateDirectParentIdTextBlock();
		}

		private void UpdateDirectParentIdTextBlock()
		{
			if (_cakRanSeqCntr == null)
			{
				return;
			}

			ifevParentId.Value = _cakRanSeqCntr.NodeBaseParams.DirectParentID.ToString();
		}
	}
}
