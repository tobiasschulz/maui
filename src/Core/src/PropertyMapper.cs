using System;
using System.Collections.Generic;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public abstract class PropertyMapper : IPropertyMapper
	{
		readonly Dictionary<string, Action<IElementHandler, IElement>> _mapper = new();

		IPropertyMapper? _chained;

		// Keep a distinct list of the keys so we don't run any duplicate (overridden) updates more than once
		// when we call UpdateProperties
		HashSet<string>? _updateKeys;

		public PropertyMapper()
		{
		}

		public PropertyMapper(IPropertyMapper chained)
		{
			Chained = chained;
		}

		public virtual void SetPropertyCore(string key, Action<IElementHandler, IElement> action)
		{
			_mapper[key] = action;
			ClearKeyCache();
		}

		public virtual void UpdatePropertyCore(string key, IElementHandler viewHandler, IElement virtualView)
		{
			var action = GetPropertyCore(key);
			action?.Invoke(viewHandler, virtualView);
		}

		public virtual Action<IElementHandler, IElement>? GetPropertyCore(string key)
		{
			if (_mapper.TryGetValue(key, out var action))
				return action;
			else if (Chained is not null)
				return Chained.GetPropertyCore(key);
			else
				return null;
		}

		public void UpdateProperty(IElementHandler viewHandler, IElement? virtualView, string property)
		{
			if (virtualView == null)
				return;

			UpdatePropertyCore(property, viewHandler, virtualView);
		}

		public void UpdateProperties(IElementHandler viewHandler, IElement? virtualView)
		{
			if (virtualView == null)
				return;

			foreach (var key in UpdateKeys)
			{
				UpdatePropertyCore(key, viewHandler, virtualView);
			}
		}

		public IPropertyMapper? Chained
		{
			get => _chained;
			set
			{
				_chained = value;
				ClearKeyCache();
			}
		}

		protected HashSet<string> PopulateKeys(ref HashSet<string>? returnList)
		{
			_updateKeys = new HashSet<string>();

			foreach (var key in GetKeys())
			{
				_updateKeys.Add(key);
			}

			return returnList ?? new HashSet<string>();
		}

		protected virtual void ClearKeyCache()
		{
			_updateKeys = null;
		}

		public virtual IReadOnlyCollection<string> UpdateKeys =>
			_updateKeys ?? PopulateKeys(ref _updateKeys);

		public IEnumerable<string> GetKeys()
		{
			foreach (var key in _mapper.Keys)
				yield return key;

			if (Chained is not null)
			{
				foreach (var key in Chained.GetKeys())
					yield return key;
			}
		}
	}

	public interface IPropertyMapper
	{
		Action<IElementHandler, IElement>? GetPropertyCore(string key);

		IEnumerable<string> GetKeys();

		void UpdateProperties(IElementHandler elementHandler, IElement virtualView);

		void UpdateProperty(IElementHandler elementHandler, IElement virtualView, string property);
	}

	public interface IPropertyMapper<out TVirtualView, out TViewHandler> : IPropertyMapper
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
	}

	public class PropertyMapper<TVirtualView, TViewHandler> : PropertyMapper, IPropertyMapper<TVirtualView, TViewHandler>
		where TVirtualView : IElement
		where TViewHandler : IElementHandler
	{
		public PropertyMapper()
		{
		}

		public PropertyMapper(IPropertyMapper chained)
			: base(chained)
		{
		}

		public Action<TViewHandler, TVirtualView> this[string key]
		{
			get
			{
				var action = GetPropertyCore(key) ?? throw new IndexOutOfRangeException($"Unable to find mapping for '{nameof(key)}'.");
				return new Action<TViewHandler, TVirtualView>((h, v) => action.Invoke(h, v));
			}
			set => Add(key, value);
		}

		public void Add(string key, Action<TViewHandler, TVirtualView> action) =>
			SetPropertyCore(key, (h, v) =>
			{
				if (v is TVirtualView vv)
					action?.Invoke((TViewHandler)h, vv);
				else
					Chained?.UpdateProperty(h, v, key);
			});
	}

	public class PropertyMapper<TVirtualView> : PropertyMapper<TVirtualView, IElementHandler>
		where TVirtualView : IElement
	{
		public PropertyMapper()
		{
		}

		public PropertyMapper(PropertyMapper chained)
			: base(chained)
		{
		}
	}
}