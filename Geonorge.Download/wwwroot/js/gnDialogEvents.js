// Wire a MutationObserver to translate the 'show' attribute into .NET events,
// and provide small helpers to open/close the dialog imperatively.

export function wire(el, dotnetRef) {
    const emit = (open) => dotnetRef.invokeMethodAsync("OnShowChangedFromDom", open);

    const mo = new MutationObserver(muts => {
        for (const m of muts) {
            if (m.type === "attributes" && m.attributeName === "show") {
                const has = el.hasAttribute("show");
                const val = has ? el.getAttribute("show") : null; // "", "true", "false", null
                const isOpen = has && val !== "false";
                emit(isOpen);
            }
        }
    });
    mo.observe(el, { attributes: true, attributeFilter: ["show"] });

    // Return a handle so we can dispose later
    return {
        dispose: () => mo.disconnect()
    };
}

export function open(el) { el.setAttribute("show", ""); }
export function close(el) { el.setAttribute("show", "false"); }
