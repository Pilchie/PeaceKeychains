# Issue: Improve Mobile Menu Accessibility

**To file this issue on GitHub, copy the content below into a new issue.**

---

## Title
Improve Mobile Menu Accessibility

## Description

The mobile menu implementation in `PeaceKeychains.Blazor/Components/Layout/MainLayout.razor` (lines 21-33) uses an HTML `<details>`/`<summary>` element which has poor accessibility support.

## Current Issues

1. **Missing ARIA attributes**: The menu lacks proper ARIA attributes for screen readers such as:
   - `aria-expanded` to indicate menu state
   - `aria-controls` to link button with menu
   - `aria-labelledby` for proper labeling

2. **Limited keyboard navigation**: The `<details>`/`<summary>` approach may have limited keyboard navigation support across different browsers and assistive technologies

3. **No click-outside handling**: The menu doesn't close when clicking outside of it, which is expected behavior for dropdown menus

## Proposed Solution

Consider implementing a proper accessible mobile menu either by:
- Adding proper ARIA attributes and JavaScript/Blazor interop for menu state management
- Using a Blazor component library that handles accessibility properly (e.g., MudBlazor, Radzen, Blazorise)
- Building a custom accessible menu component following WCAG 2.1 guidelines

## Code Location

**File:** `PeaceKeychains.Blazor/Components/Layout/MainLayout.razor`  
**Lines:** 21-33

```razor
<details class="relative">
    <summary class="list-none p-2 cursor-pointer">
        <svg class="h-6 w-6 text-gray-700" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
        </svg>
    </summary>
    <div class="absolute right-0 mt-2 w-48 bg-white rounded-lg shadow-lg py-2 z-50">
        <a href="/" class="block px-4 py-2 text-gray-700 hover:bg-peace-50">Home</a>
        <a href="/submit" class="block px-4 py-2 text-gray-700 hover:bg-peace-50">Submit</a>
        <a href="/about" class="block px-4 py-2 text-gray-700 hover:bg-peace-50">About</a>
        <a href="/privacy" class="block px-4 py-2 text-gray-700 hover:bg-peace-50">Privacy</a>
    </div>
</details>
```

## References

- [WCAG 2.1 Guidelines for Menu Accessibility](https://www.w3.org/WAI/WCAG21/Understanding/)
- [ARIA: menu role](https://developer.mozilla.org/en-US/docs/Web/Accessibility/ARIA/Roles/menu_role)
- [WAI-ARIA Authoring Practices: Menu Button Pattern](https://www.w3.org/WAI/ARIA/apg/patterns/menubutton/)

## Suggested Labels

- `accessibility`
- `enhancement`
- `ui`
- `a11y`

## Related

- Original PR: #24
- Review comment: https://github.com/Pilchie/PeaceKeychains/pull/24#discussion_r2739325230
- Fix commit: 856a51c (Added TODO comment to track this issue)
