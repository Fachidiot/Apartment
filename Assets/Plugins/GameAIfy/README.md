# GameAIfy Unity SDK

[GameAIfy](https://gameaify.com/sign-in) UnitySDK is designed for integrating GameAIfy into your Unity projects.

---

## 1. How to Import the Package

1. **Import the Package:**

   - Either double-click the `.unitypackage` file to open the import window
   - Or go to the Unity Editor menu **Assets → Import Package → Custom Package** and select the package.

2. **Install/Upgrade Dependencies:**  
   Click the **Install/Upgrade** button to download any required dependencies, then press the **Import** button to complete the package import.

3. **Verify Import:**  
   After a successful import, you should see a new **GameAIfy** folder under your **Assets** directory.

4. **Configure API Keys:**  
   Open **Window → GameAIfy → Settings** from the Unity Editor’s top menu.  
   Enter your issued **API Key** and **Secret Key** from GameAIfy, then click **Save**.

5. **Generate Assets:**  
   You can now create images by navigating to **Window → GameAIfy → Generate Assets**.

---

## 2. How to Use the GameAIfy Package

1. **Select a Model:**  
   Click the **Select Model First** button to choose the desired Lora model that best fits the image style you want to generate.

2. **Enter Positive Prompt:**  
   Type your desired prompt into the **Positive Prompt** text box.

   > **Tip:** Use the **Random** button to generate a random prompt that suits the selected model.

3. **Generate the Image:**  
   Press the **Generate** button to start image creation.

4. **Optional Settings for Enhanced Images:**  
   To further refine your image generation, you can:
   - Use the **Negative Prompt** to exclude unwanted elements such as low-quality details or inserted text.
   - Add a **Reference Image** by dragging and dropping an image; this will guide the generation process.
   - Adjust the **Image Count** to specify how many images to generate at once.
   - Choose the **Aspect Ratio** (Portrait, Landscape, or Square) that suits your needs.
