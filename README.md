# Pillz

## Running the Server


1. **Install SpaceTimeDB CLI:**
   ```sh
    curl -sSf https://install.spacetimedb.com | sh   
   ```
   
2. **Start SpaceTimeDB:**
    ```sh
    spacetimedb start
    ```

3. **Generate C# bindings for the client:**
   ```sh
   ./generate.sh
   ```

4. **Publish the server locally:**
   ```sh
   ./publish.sh
   ```

5. **View server logs:**
   ```sh
   ./logs.sh
   ```

6. **(Optional) Expose the server via ngrok:**
   ```sh
   ./ngrok.sh
   ```

## Running the Unity Client

1. Open the `client` folder in Unity Editor.
2. Ensure the generated scripts are present in `client/Assets/Scripts/autogen`.
3. Press Play in Unity to start the client.

## Requirements

- Rust and Cargo installed for server development.
- Unity Editor for client development.
- Ngrok (optional) for public server access.
