# Validation

CI and routine repository checks require no proprietary content. Synthetic GFF,
indexed-image, palette, indexed-font, and DSIX fixtures exercise successful decoding plus
truncation, bounds, invalid-component, unsafe-path, inventory, and transactional
replacement failures. Core rule tests use explicit inputs and no ambient state.

The smoke modes have distinct purposes:

- `--smoke-test` exits before content or graphics initialization and is safe on
  a content-free CI worker.
- `--content-smoke-test --asset-pack <path>` verifies the exact pack inventory,
  opens all five DSIX startup assets, and checks their frame/geometry contracts
  without a window.
- `--platform-smoke-test` creates the MonoGame platform surface and exits; it is
  reserved for installed-package environments with a display server.
- Normal startup verifies the pack before opening a window and renders the
  extracted title and start-window controls.

Owned-build validation currently targets GOG product `1432903719`, installed
build `52095422060333615`. Local-only decoded previews established the title
and start-window mappings recorded as `DATA-GOG-TITLE-001` and
`DATA-GOG-UI-001`; screenshots and decoded outputs stay
under ignored `analysis/original/` and never become golden files. Presentation
goldens in Git must use synthetic stand-ins. Visual comparison, input traces,
animation timing, and audiovisual synchronization remain open and will be
recorded per parity row rather than inferred from passing parsers.
