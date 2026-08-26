import SwiftUI

/// The robot. Body and shell are shared; only what sits inside the visor
/// changes between faces, so a new face is one `case` in `FaceView`.
struct RobotView: View {
    let face: Face
    let accent: Color
    let scale: Double
    let blink: Bool

    @State private var bob = false

    private var s: CGFloat { CGFloat(scale) }

    private var shell: LinearGradient {
        LinearGradient(colors: [Color(white: 0.99), Color(white: 0.86)],
                       startPoint: .top, endPoint: .bottom)
    }

    var body: some View {
        VStack(spacing: 0) {
            antenna
            head
            Rectangle().fill(Color(white: 0.72)).frame(width: 8 * s, height: 4 * s)  // neck
            torso
        }
        .shadow(color: .black.opacity(0.28), radius: 10 * s, y: 5 * s)
        .offset(y: bob ? -3 * s : 3 * s)
        .animation(.easeInOut(duration: 1.6).repeatForever(autoreverses: true), value: bob)
        .onAppear { bob = true }
    }

    private var antenna: some View {
        ZStack(alignment: .bottom) {
            Capsule().fill(Color(white: 0.72))
                .frame(width: 2.5 * s, height: 12 * s)
                .offset(y: -5 * s)
            Circle().fill(accent)
                .frame(width: 8 * s, height: 8 * s)
                .shadow(color: accent.opacity(0.9), radius: 6 * s)
                .offset(y: -14 * s)
        }
        .frame(height: 16 * s)
    }

    private var head: some View {
        ZStack {
            RoundedRectangle(cornerRadius: 13 * s, style: .continuous)
                .fill(shell)
                .overlay(
                    RoundedRectangle(cornerRadius: 13 * s, style: .continuous)
                        .stroke(Color.black.opacity(0.10), lineWidth: 1)
                )
                .frame(width: 54 * s, height: 44 * s)

            RoundedRectangle(cornerRadius: 9 * s, style: .continuous)
                .fill(Color(white: 0.13))
                .frame(width: 44 * s, height: 30 * s)
                .overlay(FaceView(face: face, accent: accent, scale: scale, blink: blink))
                .overlay(alignment: .top) {
                    RoundedRectangle(cornerRadius: 9 * s, style: .continuous)
                        .fill(LinearGradient(colors: [.white.opacity(0.18), .clear],
                                             startPoint: .top, endPoint: .bottom))
                        .frame(height: 14 * s)
                        .padding(.horizontal, 3 * s)
                        .padding(.top, 2 * s)
                }
        }
        .overlay(alignment: .leading) { ear.offset(x: -2 * s) }
        .overlay(alignment: .trailing) { ear.offset(x: 2 * s) }
    }

    private var ear: some View {
        Capsule().fill(Color(white: 0.78)).frame(width: 5 * s, height: 14 * s)
    }

    private var torso: some View {
        ZStack {
            RoundedRectangle(cornerRadius: 10 * s, style: .continuous)
                .fill(shell)
                .overlay(
                    RoundedRectangle(cornerRadius: 10 * s, style: .continuous)
                        .stroke(Color.black.opacity(0.10), lineWidth: 1)
                )
                .frame(width: 42 * s, height: 26 * s)
            Circle().fill(accent.opacity(0.9))
                .frame(width: 7 * s, height: 7 * s)
                .shadow(color: accent.opacity(0.8), radius: 4 * s)
        }
        .overlay(alignment: .leading) { arm.offset(x: -4 * s, y: -1 * s) }
        .overlay(alignment: .trailing) { arm.offset(x: 4 * s, y: -1 * s) }
    }

    private var arm: some View {
        Capsule().fill(Color(white: 0.80)).frame(width: 5 * s, height: 15 * s)
    }
}

/// What shows inside the visor. Add a face: add a `case` here and to `Face`.
struct FaceView: View {
    let face: Face
    let accent: Color
    let scale: Double
    let blink: Bool

    private var s: CGFloat { CGFloat(scale) }
    /// Blinking squashes the eyes vertically — shared by every face.
    private var lid: CGFloat { blink ? 0.12 : 1 }

    var body: some View {
        Group {
            switch face {
            case .visor:   visor
            case .cyclops: cyclops
            case .pixel:   pixel
            case .angry:   angry
            case .dot:     dot
            }
        }
        .animation(.easeInOut(duration: 0.09), value: blink)
    }

    // Two glowing capsules — the original.
    private var visor: some View {
        HStack(spacing: 9 * s) {
            eye(Capsule(), 8, 12)
            eye(Capsule(), 8, 12)
        }
    }

    // One big lens with a darker iris ring.
    private var cyclops: some View {
        ZStack {
            Circle().fill(accent).frame(width: 17 * s, height: 17 * s)
                .shadow(color: accent.opacity(0.9), radius: 6 * s)
            Circle().stroke(Color.black.opacity(0.45), lineWidth: 2.5 * s)
                .frame(width: 17 * s, height: 17 * s)
            Circle().fill(Color.black.opacity(0.55)).frame(width: 5 * s, height: 5 * s)
        }
        .scaleEffect(y: lid, anchor: .center)
    }

    // 8-bit: square eyes over a three-block mouth.
    private var pixel: some View {
        VStack(spacing: 3.5 * s) {
            HStack(spacing: 7 * s) {
                eye(Rectangle(), 7, 7)
                eye(Rectangle(), 7, 7)
            }
            HStack(spacing: 2 * s) {
                ForEach(0..<3, id: \.self) { _ in
                    Rectangle().fill(accent.opacity(0.75)).frame(width: 4 * s, height: 3 * s)
                }
            }
        }
    }

    // Angled brows over narrowed eyes.
    private var angry: some View {
        ZStack {
            HStack(spacing: 9 * s) {
                eye(Capsule(), 8, 9)
                eye(Capsule(), 8, 9)
            }
            .offset(y: 2 * s)
            HStack(spacing: 7 * s) {
                brow.rotationEffect(.degrees(18))
                brow.rotationEffect(.degrees(-18))
            }
            .offset(y: -7 * s)
        }
    }

    private var brow: some View {
        Capsule().fill(accent).frame(width: 12 * s, height: 3 * s)
            .shadow(color: accent.opacity(0.7), radius: 3 * s)
    }

    // Round eyes and a smile.
    private var dot: some View {
        VStack(spacing: 3 * s) {
            HStack(spacing: 9 * s) {
                eye(Circle(), 7, 7)
                eye(Circle(), 7, 7)
            }
            Smile().stroke(accent, style: StrokeStyle(lineWidth: 2.2 * s, lineCap: .round))
                .frame(width: 16 * s, height: 6 * s)
                .shadow(color: accent.opacity(0.7), radius: 3 * s)
        }
    }

    private func eye<S: Shape>(_ shape: S, _ w: CGFloat, _ h: CGFloat) -> some View {
        shape.fill(accent)
            .frame(width: w * s, height: h * s)
            .scaleEffect(y: lid, anchor: .center)
            .shadow(color: accent.opacity(0.9), radius: 5 * s)
    }
}

struct Smile: Shape {
    func path(in rect: CGRect) -> Path {
        var p = Path()
        p.move(to: CGPoint(x: rect.minX, y: rect.minY))
        p.addQuadCurve(to: CGPoint(x: rect.maxX, y: rect.minY),
                       control: CGPoint(x: rect.midX, y: rect.maxY * 1.8))
        return p
    }
}
