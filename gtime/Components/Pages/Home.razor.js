/* ------------------------------------------------------------------ *
 * Helpers                                                             *
 * ------------------------------------------------------------------ */
const DAY_MIN = 24 * 60;
const COLORS   = { active : 'var(--active)', afk : 'var(--afk)' };
let timelineBlocks;

const $ = (sel, ctx = document) => ctx.querySelector(sel);

const hhmmToMin = t => {
    const [h, m] = t.split(':').map(Number);
    return h * 60 + m;
};

const fmt = min => {
    const h = Math.floor(min / 60);
    const m = min % 60;
    return `${h}:${String(m).padStart(2, '0')}`;
};

/* ------------------------------------------------------------------ *
 * Tooltip                                                             *
 * ------------------------------------------------------------------ */
let tooltip = $('#floating-infobox');
const showTip  = (txt, x, y) => {
    tooltip = $('#floating-infobox');
    tooltip.textContent = txt;
    tooltip.style.display = 'block';
    console.log(txt, x, y);
    console.log(tooltip.style.display);
    moveTip(x, y);
};

const moveTip = (x, y) => {
    tooltip = $('#floating-infobox');
    tooltip.style.left = x + 12 + 'px';
    tooltip.style.top  = y + 12 + 'px';
};

const hideTip = () => { tooltip.style.display = 'none'; };

/* ------------------------------------------------------------------ *
 * Build timeline                                                      *
 * ------------------------------------------------------------------ */
function renderBlocks() {
    const bar = $('#timeline');

    timelineBlocks.forEach(evt => {
        const startMin = hhmmToMin(evt.start);
        const endMin   = hhmmToMin(evt.end);
        const block    = document.createElement('div');

        block.className       = 'activity';
        block.style.left      = (startMin / DAY_MIN) * 100 + '%';
        block.style.width     = ((endMin - startMin) / DAY_MIN) * 100 + '%';
        block.style.background = COLORS[evt.type];

        const tipText = `${evt.title}\n${evt.description}`;

        // Tooltip events
        block.addEventListener('mouseenter', e => showTip(tipText, e.pageX, e.pageY));
        block.addEventListener('mousemove',  e => moveTip(e.pageX, e.pageY));
        block.addEventListener('mouseleave', hideTip);

        bar.appendChild(block);
    });
    document.querySelector('#loader-container').style.display = 'none';
    document.querySelector('#timeline-wrapper').style.display = 'block';
}

/* ------------------------------------------------------------------ *
 * Drag‑selection                                                      *
 * ------------------------------------------------------------------ */
function enableDragSelection() {
    const bar = $('#timeline');
    let   down   = false;
    let   startX = 0;

    // Overlay elements created once
    const sel  = Object.assign(document.createElement('div'), { className: 'selection', style: 'display:none;' });
    const info = Object.assign(document.createElement('div'), { className: 'infobox',  style: 'display:none;' });
    bar.append(sel, info);

    bar.addEventListener('mousedown', e => {
        down   = true;
        startX = e.clientX;
        sel.style.display  = 'block';
        info.style.display = 'block';
        update(e.clientX);
    });

    document.addEventListener('mousemove', e => { if (down) update(e.clientX); });

    document.addEventListener('mouseup', () => {
        if (!down) return;
        down = false;
        sel.style.display  = 'none';
        info.style.display = 'none';
    });

    function update(currentX) {
        const rect = bar.getBoundingClientRect();
        const x1   = Math.max(rect.left, Math.min(startX, currentX));
        const x2   = Math.min(rect.right, Math.max(startX, currentX));

        // Render overlay
        sel.style.left  = ((x1 - rect.left) / rect.width) * 100 + '%';
        sel.style.width = ((x2 - x1) / rect.width) * 100 + '%';

        // Time maths
        const selStart = ((x1 - rect.left) / rect.width) * DAY_MIN;
        const selStartHM = [Math.floor(selStart / 60), Math.floor(selStart % 60)].map(n => n.toString().padStart(2, '0')).join(':');
        const selEnd   = ((x2 - rect.left) / rect.width) * DAY_MIN;
        const selEndHM = [Math.floor(selEnd / 60), Math.floor(selEnd % 60)].map(n => n.toString().padStart(2, '0')).join(':');
        const selDur   = Math.round(selEnd - selStart);
        
        // Active minutes within selection
        let activeMin = 0;
        timelineBlocks.forEach(evt => {
            if (evt.type !== 'active') return;
            const aStart = hhmmToMin(evt.start);
            const aEnd   = hhmmToMin(evt.end);
            activeMin += Math.max(0, Math.min(selEnd, aEnd) - Math.max(selStart, aStart));
        });
        activeMin = Math.round(activeMin);

        // Update info bubble
        info.textContent = `total ${fmt(selDur)} | active ${fmt(activeMin)} | start ${selStartHM} end ${selEndHM}`;
        info.style.left  = ((x1 + x2) / 2 - rect.left) + 'px';
    }
}

/* ------------------------------------------------------------------ *
 * Init                                                                 *
 * ------------------------------------------------------------------ */
export function initTimeline(entries) {
    console.log("init");
    timelineBlocks = entries;
    renderBlocks();
    enableDragSelection();
}