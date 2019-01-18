/*
    This code belongs to the "EyeSkills Framework". It is designed to assist
    in the development of software for the human/animal visual system. 

    Copyright(C) 2018 Dr. Thomas Benjamin Senior, Michael Zöller.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program. If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using PowerUI;
using UnityEngine;

public static class EyeSkillsDragScroller{
	
	/// <summary>The starting point for the scrolling.</summary>
	private static float StartY;
	/// <summary>The element currently being scrolled.</summary>
	public static HtmlElement Scrolling;
	
	
	/// <summary>True if this is currently scrolling.</summary>
	public static bool IsScrolling{
		get{
			return (Scrolling!=null);
		}
	}
	
	/// <summary>This gets called when the element is clicked on.
	/// It's onmousedown points at this method.</summary>
	[Values.Preserve]
	public static void StartScroll(UIEvent e){
		
		if(!e.isLeftMouse){
			// Not a left click.
			return;
		}
		
		// Store the element being scrolled:
		// (Cast to a HtmlElement for focus() etc):
		Scrolling=(e.currentTarget as HtmlElement);
		
		// Store the current position of the mouse, and the current scroll offset.
		// We want to find how many pixels it's moved by since it went down:
		StartY=e.clientY + Scrolling.scrollTop;
		
		Scrolling.focus();
		
	}
	
	/// <summary>Called when the element scrolls.</summary>
	[Values.Preserve]
	public static void ScrollNow(UIEvent e){
		
		if(Scrolling==null){
			return;
		}
		
		// How much has the mouse moved by?
		float scrollTo=StartY-e.clientY;
		
		// Whats the furthest it can go?
		// The height of the stuff inside the box - the actual height of the box.
		float limit=Scrolling.contentHeight-Scrolling.scrollHeight;
		
		if(limit<0){
			limit=0;
		}
		
		// Clip it by the limits:
		if(scrollTo<0){
			// Dragged the content down really far.
			scrollTo=0;
		}else if(scrollTo>limit){
			// Dragged the content up too far.
			scrollTo=limit;
		}
		
		// Get current top:
		float top=Scrolling.scrollTop;
		
		// Get the delta:
		float scrolledBy=scrollTo-top;
		
		// Scroll by that much:
		Scrolling.scrollBy(0,scrolledBy);
		
		// Check if the mouse went up:
		if(!e.leftMouseDown){
			
			// -- Inertial Scrolling --
			
			// Running an animation here could be done to create 'drift' where the scrolling decelerates to a smooth stop.
			// Highly reccommended to use the "scroll" CSS property.
			
			// For example, something like the following (Rough!):
			
			/*
			// Get the px value to scroll to:
			float targetScroll=top + scrolledBy;
			
			// Animate the scrolling there:
			// -> Note that the last '1' means decelerate for 1 second.
			Scrolling.animate("scroll-top:"+targetScroll+"px",1f);
			*/
			
			// Alternatively, delay the unfocus and have a speed value, then affect the speed from this function to have the same effect.
			
			// Left button isn't down - clear it:
			Scrolling=null;
			
		}
		
	}
	
}