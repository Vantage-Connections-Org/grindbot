import SwiftUI

/// Rounded rect with a tail on one side, drawn as a single continuous path so
/// the outline has no seam where the tail meets the body.
struct BubbleShape: Shape {
    let scale: Double
    let flipped: Bool   // true = tail on the left, for left-hand screen corners

    private var s: CGFloat { CGFloat(scale) }

    func path(in rect: CGRect) -> Path {
        let r = 17 * s
        let tail = 11 * s
        let bodyMaxX = rect.maxX - tail
        let tailTop = rect.maxY - 36 * s

        var p = Path()
        p.move(to: CGPoint(x: rect.minX + r, y: rect.minY))
        p.addLine(to: CGPoint(x: bodyMaxX - r, y: rect.minY))
        p.addArc(tangent1End: CGPoint(x: bodyMaxX, y: rect.minY),
                 tangent2End: CGPoint(x: bodyMaxX, y: rect.minY + r), radius: r)
        p.addLine(to: CGPoint(x: bodyMaxX, y: tailTop))
        p.addLine(to: CGPoint(x: rect.maxX, y: tailTop + 10 * s))
        p.addLine(to: CGPoint(x: bodyMaxX, y: tailTop + 19 * s))
        p.addLine(to: CGPoint(x: bodyMaxX, y: rect.maxY - r))
        p.addArc(tangent1End: CGPoint(x: bodyMaxX, y: rect.maxY),
                 tangent2End: CGPoint(x: bodyMaxX - r, y: rect.maxY), radius: r)
        p.addLine(to: CGPoint(x: rect.minX + r, y: rect.maxY))
        p.addArc(tangent1End: CGPoint(x: rect.minX, y: rect.maxY),
                 tangent2End: CGPoint(x: rect.minX, y: rect.maxY - r), radius: r)
        p.addLine(to: CGPoint(x: rect.minX, y: rect.minY + r))
        p.addArc(tangent1End: CGPoint(x: rect.minX, y: rect.minY),
                 tangent2End: CGPoint(x: rect.minX + r, y: rect.minY), radius: r)
        p.closeSubpath()

        guard flipped else { return p }
        return p.applying(CGAffineTransform(scaleX: -1, y: 1)
            .translatedBy(x: -rect.width, y: 0))
    }
}

struct PopupView: View {
    /// Window size at scale 1. AppDelegate sizes the window from the same numbers.
    static let baseSize = CGSize(width: 440, height: 230)

    @ObservedObject var model: PopupModel
    @ObservedObject var settings: Settings

    private var cfg: Config { settings.cfg }
    private var s: CGFloat { CGFloat(cfg.scale) }
    private var onLeft: Bool { cfg.position.isLeft }

    var body: some View {
        content
            .padding(.horizontal, CGFloat(cfg.margin) * s / 1.5)
            .padding(.vertical, 18 * s)
            // Definite size, or NSHostingView grows the window to its intrinsic
            // height and pushes the robot off the bottom of the screen.
            .frame(width: PopupView.baseSize.width * s,
                   height: PopupView.baseSize.height * s,
                   alignment: cfg.position.alignment)
            .opacity(model.visible ? 1 : 0)
            .scaleEffect(model.visible ? 1 : 0.9, anchor: cfg.position.anchor)
            .offset(y: model.visible ? 0 : (cfg.position.isTop ? -16 * s : 16 * s))
    }

    /// Robot hugs the screen edge, bubble points back at it — so the whole
    /// arrangement mirrors for left-hand corners.
    @ViewBuilder private var content: some View {
        HStack(alignment: .bottom, spacing: 8 * s) {
            if onLeft {
                robot
                bubble
            } else {
                bubble
                robot
            }
        }
    }

    private var robot: some View {
        RobotView(face: cfg.face, accent: cfg.accentColor, scale: cfg.scale, blink: model.blink)
            .frame(width: 64 * s)
    }

    private var bubble: some View {
        Text(model.shownText)
            .font(.system(size: 14.5 * s, weight: .medium, design: .rounded))
            .foregroundStyle(.primary)
            .multilineTextAlignment(.leading)
            .fixedSize(horizontal: false, vertical: true)
            .padding(.vertical, 12 * s)
            .padding(.leading, (onLeft ? 24 : 16) * s)
            .padding(.trailing, (onLeft ? 16 : 24) * s)
            .frame(maxWidth: CGFloat(cfg.maxBubbleWidth) * s, alignment: .leading)
            .background(
                BubbleShape(scale: cfg.scale, flipped: onLeft)
                    .fill(.regularMaterial)
                    .overlay(
                        BubbleShape(scale: cfg.scale, flipped: onLeft)
                            .stroke(Color.white.opacity(0.22), lineWidth: 1)
                    )
                    .shadow(color: .black.opacity(0.25), radius: 14 * s, y: 6 * s)
            )
            .padding(.bottom, 14 * s)
    }
}
